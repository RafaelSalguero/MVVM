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
        private readonly IQueryable<T> original;
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
        private readonly IQueryable<T> original;
        private ConstantExpression originalExpression;
        public ClientSideQuery(IQueryable<T> original)
        {
            this.Expression = this.originalExpression = Expression.Constant(original);
            this.ElementType = original.ElementType;
            Provider = new QP(originalExpression);
        }

        private ClientSideQuery()
        {
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
                var Ret = new ClientSideQuery<TElement>();
                Ret.ElementType = typeof(TElement);
                Ret.Expression = expression;
                Ret.Provider = new QP(originalExpression);
                Ret.originalExpression = originalExpression;
                return Ret;
            }

            public object Execute(Expression expression)
            {
                throw new NotImplementedException();
            }

            public TResult Execute<TResult>(Expression expression)
            {
                throw new NotImplementedException();
            }
        }

        public Type ElementType
        {
            get; private set;

        }

        public Expression Expression
        {
            get; private set;
        }

        public IQueryProvider Provider
        {
            get; private set;
        }


        public IEnumerator<T> GetEnumerator()
        {
            var originalType = originalExpression.Value.GetType().GetGenericArguments()[0];
            var ToList = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.ToList) && x.IsGenericMethod).First().MakeGenericMethod(originalType);
            var AsQueryable = typeof(Queryable).GetMethods().Where(x => x.Name == nameof(Queryable.AsQueryable) && x.IsGenericMethod).First().MakeGenericMethod(originalType);

            var c1 = Expression.Call(ToList, originalExpression);
            var c2 = Expression.Call(AsQueryable, c1);
            var Replace = ReplaceVisitor.Replace(Expression, originalExpression, c2);


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
