using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    class ServerSideQuery<T> : IQueryable<T>
    {
        internal readonly IQueryable<T> original;
        public ServerSideQuery(IQueryable<T> original)
        {
            this.original = original;
        }

        public Type ElementType
        {
            get
            {
                return original.ElementType;
            }
        }

        public Expression Expression
        {
            get
            {
                return original.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return original.Provider;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return original.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class ClientSideQuery<T> : IQueryable<T>
    {
        enum QueryType
        {
            /// <summary>
            /// La ejecución del query sera ejecutar primero la parte del servidor, luego aplicar el resto del query
            /// </summary>
            Enumerable,

            /// <summary>
            /// El query es uno o varios selects sobre un query del servidor, por lo que algunas operaciones se pueden delegar al servidor, aunque se realizen despues del select
            /// </summary>
            SelectOnly
        }

        private readonly IQueryable<T> original;
        private ConstantExpression originalExpression;
        public ClientSideQuery(IQueryable<T> original)
        {
            this.Expression = this.originalExpression = Expression.Constant(original);
            Provider = new QP(originalExpression);
        }

        private ClientSideQuery(Expression Ex, ConstantExpression originalEx)
        {
            this.Expression = Ex;
            this.originalExpression = originalEx;
            Provider = new QP(originalExpression);

            qType = QueryType.Enumerable;
            if (IsSelect(Ex, originalEx))
            {
                qType = QueryType.SelectOnly;
            }
        }

        readonly QueryType qType;

        private static bool IsSelect(Expression Ex, ConstantExpression originalEx)
        {
            while (Ex == originalEx || IsSelect(Ex, out Ex))
            {
                if (Ex == originalEx)
                    return true;
            }
            return false;
        }
        private static bool IsSelect(Expression Ex, out Expression Input)
        {
            Input = null;
            if (Ex is MethodCallExpression)
            {
                var aux = (MethodCallExpression)Ex;
                if (aux.Method.IsGenericMethod)
                {
                    var SelectMethod = typeof(Queryable).GetMethods().Where(x => x.Name == nameof(Queryable.Select) && x.IsGenericMethod);
                    if (SelectMethod.Contains(aux.Method.GetGenericMethodDefinition()))
                    {
                        Input = aux.Arguments[0];
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsServerDeferrable(Expression Ex)
        {
            if (Ex is MethodCallExpression)
            {
                var aux = (MethodCallExpression)Ex;
                if (aux.Method.IsGenericMethod)
                {
                    var TakeMethod = typeof(Queryable).GetMethods().Where(x => x.Name == nameof(Queryable.Take) && x.IsGenericMethod).Single();
                    var SkipMethod = typeof(Queryable).GetMethods().Where(x => x.Name == nameof(Queryable.Skip) && x.IsGenericMethod).Single();
                    var CountMethod = typeof(Queryable).GetMethods().Where(x => x.Name == nameof(Queryable.Count) && x.IsGenericMethod && x.GetParameters().Length == 1).Single();

                    var M = aux.Method.GetGenericMethodDefinition();
                    return (M == TakeMethod || M == SkipMethod || M == CountMethod);
                }
            }
            return false;
        }

        private static bool IsServerDeferrable(Expression Ex, ConstantExpression originalExpression)
        {
            if (IsServerDeferrable(Ex))
            {
                var Ar = ((MethodCallExpression)Ex).Arguments[0];
                return IsSelect(Ar, originalExpression);
            }
            return false;
        }

        private static Expression ServerDeferrExpression(Expression Ex, ConstantExpression OriginalEx)
        {
            var OldServerQuery = (IQueryable)OriginalEx.Value;

            var ExMethod = ((MethodCallExpression)Ex);
            var NewMethod = ExMethod.Method.GetGenericMethodDefinition().MakeGenericMethod(OldServerQuery.ElementType);

            var NewArguments = new Expression[] { OldServerQuery.Expression }.Concat(ExMethod.Arguments.Skip(1)).ToArray();
            var NewServerQuery = Expression.Call(NewMethod, NewArguments);

            return NewServerQuery;
        }

        private static void ServerDeferrQuery(Expression Ex, ConstantExpression OriginalEx, out Expression NewEx, out ConstantExpression NewOriginalEx)
        {
            var OldServerQuery = (IQueryable)OriginalEx.Value;

            var ServerEx = ServerDeferrExpression(Ex, OriginalEx);
            var Query = OldServerQuery.Provider.CreateQuery(ServerEx);

            var newOriginalQuery = Activator.CreateInstance(typeof(ServerSideQuery<>).MakeGenericType(OldServerQuery.ElementType), Query);

            NewOriginalEx = Expression.Constant(newOriginalQuery);

            NewEx = ((MethodCallExpression)Ex).Arguments[0];
            NewEx = ReplaceVisitor.Replace(NewEx, OriginalEx, NewOriginalEx);
        }

        class QP : IQueryProvider
        {
            private readonly ConstantExpression originalExpression;
            public QP(ConstantExpression originalExpression)
            {
                this.originalExpression = originalExpression;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                throw new NotImplementedException();
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                if (IsServerDeferrable(expression, originalExpression))
                {
                    Expression NewEx;
                    ConstantExpression NewOriginalEx;
                    ServerDeferrQuery(expression, originalExpression, out NewEx, out NewOriginalEx);

                    var ret = new ClientSideQuery<TElement>(NewEx, NewOriginalEx);
                    return ret;
                    //return new ClientSideQuery<TElement>(ServerDeferr(expression, originalExpression), originalExpression); ;
                }
                else
                {
                    return new ClientSideQuery<TElement>(expression, originalExpression); ;
                }
            }

            public object Execute(Expression expression)
            {
                throw new NotImplementedException();
            }

            public TResult Execute<TResult>(Expression expression)
            {
                if (IsServerDeferrable(expression, originalExpression))
                {
                    //Ejecuta la funcion de agregado en el lado del servidor, ignorando completamente el lado del cliente
                    //La unica funcion que se puede ejecutar de esta manera es el Count()

                    var Ex = ServerDeferrExpression(expression, originalExpression);
                    var OldServerQuery = (IQueryable)originalExpression.Value;
                    var Result = OldServerQuery.Provider.Execute<TResult>(Ex);

                    return Result;
                }
                else
                {
                    //Ejecuta todo el lado del servidor, y luego el agregado, en el lado del cliente
                    var Executable = CreateExecutable(expression, originalExpression);
                    var Ex = Expression.Lambda(Executable).Compile().DynamicInvoke();

                    return (TResult)Ex;
                }
            }
        }

        public Type ElementType
        {
            get
            {
                return typeof(T);
            }

        }

        public Expression Expression
        {
            get; private set;
        }

        public IQueryProvider Provider
        {
            get; private set;
        }

        public static Expression CreateExecutable(Expression Expression, ConstantExpression originalExpression)
        {
            var originalType = originalExpression.Value.GetType().GetGenericArguments()[0];
            var ToList = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.ToList) && x.IsGenericMethod).First().MakeGenericMethod(originalType);
            var AsQueryable = typeof(Queryable).GetMethods().Where(x => x.Name == nameof(Queryable.AsQueryable) && x.IsGenericMethod).First().MakeGenericMethod(originalType);

            var c1 = Expression.Call(ToList, originalExpression);
            var c2 = Expression.Call(AsQueryable, c1);
            var Replace = ReplaceVisitor.Replace(Expression, originalExpression, c2);

            return Replace;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var Replace = CreateExecutable(Expression, originalExpression);


            var L = Expression.Lambda(Replace);
            var Ret = L.Compile().DynamicInvoke();
            return ((IEnumerable<T>)Ret).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
