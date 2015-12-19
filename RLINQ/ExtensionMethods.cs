using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tonic.ExpressionExecute;

namespace Tonic
{


    /// <summary>
    /// Provides extension methods for decompiling expression trees and IQueryables
    /// </summary>
    public static class ExtensionMethods
    {


        [ThreadStatic]
        internal static bool ThrowExecuteExpression;
        internal class ExpressionException : Exception
        {
            public ExpressionException(LambdaExpression Value)
            {
                this.Value = Value;
            }
            public LambdaExpression Value { get; private set; }
        }


        public static Expression<Func<TInput, TResult>> Expr<TInput, TResult>(this TInput Input, Expression<Func<TInput, TResult>> Expr)
        {
            return Expr;
        }


        [ThreadStatic]
        private static Dictionary<Delegate, Tuple<Delegate, LambdaExpression>> expressionExecuteCache = null;

        /// <summary>
        /// Executes a one argument expression given the argument, the expression compilation is cached.
        /// Use this method with the Select method
        /// </summary>
        /// <typeparam name="TInput">The type of te input argument</typeparam>
        /// <typeparam name="TResult">Resulting type</typeparam>
        /// <param name="Expression">The expression to execute</param>
        /// <param name="Input">The input argument</param>
        /// <returns>The result of the expression execution</returns>
        public static TResult Invoke<TInput, TResult>(this TInput Input, Func<ExpressionFluent<TInput>, Expression<Func<TInput, TResult>>> Expression)
        {
            if (expressionExecuteCache == null)
                expressionExecuteCache = new Dictionary<Delegate, Tuple<Delegate, LambdaExpression>>();

            Tuple<Delegate, LambdaExpression> CacheValue;
            if (!expressionExecuteCache.TryGetValue(Expression, out CacheValue))
            {
                //Calcula por primera vez los valores del cache:
                var Expr = Expression(new ExpressionFluent<TInput>(Input));
                if (Expr == null)
                    throw new ArgumentException("La expresión devuelta por el delegado Expression no puede ser nula");

                var Comp = Expr.Compile();
                CacheValue = Tuple.Create((Delegate)Comp, (LambdaExpression)Expr);
                expressionExecuteCache.Add(Expression, CacheValue);
            }

            if (ThrowExecuteExpression)
                throw new ExpressionException(CacheValue.Item2);

            var Func = (Func<TInput, TResult>)CacheValue.Item1;
            return Func(Input);
        }


        /// <summary>
        /// Executes a one argument expression given the argument, the expression compilation is NOT cached
        /// </summary>
        /// <typeparam name="TInput">The type of te input argument</typeparam>
        /// <typeparam name="TResult">Resulting type</typeparam>
        /// <param name="Expression">The expression to execute</param>
        /// <param name="Input">The input argument</param>
        /// <returns>The result of the expression execution</returns>
        [Obsolete("Este método es ineficiente, utilize la sobrecarga this.Invoke(e => e.Select ( expr ) )")]
        public static TResult Execute<TInput, TResult>(this TInput Input, Expression<Func<TInput, TResult>> Expression)
        {
            if (ThrowExecuteExpression)
                throw new ExpressionException(Expression);

            Func<object, object> ret;

            var Compile = Expression.Compile();
            ret = o => Compile((TInput)o);

            return (TResult)ret(Input);
        }

        #region Decompile overloads
        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression">Expression to decompile</param>
        public static Expression<Func<TResult>> Decompile<TResult>(this Expression<Func<TResult>> Expression)
        {
            return ((Expression<Func<TResult>>)(DecompilerVisitor.Decompile(Expression)));
        }

        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression">Expression to decompile</param>
        public static Expression<Func<T, TResult>> Decompile<T, TResult>(this Expression<Func<T, TResult>> Expression)
        {
            return ((Expression<Func<T, TResult>>)(DecompilerVisitor.Decompile(Expression)));
        }

        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression">Expression to decompile</param>
        public static Expression<Func<T1, T2, TResult>> Decompile<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> Expression)
        {
            return ((Expression<Func<T1, T2, TResult>>)(DecompilerVisitor.Decompile(Expression)));
        }

        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression">Expression to decompile</param>
        public static Expression<Func<T1, T2, T3, TResult>> Decompile<T1, T2, T3, TResult>(this Expression<Func<T1, T2, T3, TResult>> Expression)
        {
            return ((Expression<Func<T1, T2, T3, TResult>>)(DecompilerVisitor.Decompile(Expression)));
        }


        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression">Expression to decompile</param>
        public static Expression<Func<T1, T2, T3, T4, TResult>> Decompile<T1, T2, T3, T4, TResult>(this Expression<Func<T1, T2, T3, T4, TResult>> Expression)
        {
            return ((Expression<Func<T1, T2, T3, T4, TResult>>)(DecompilerVisitor.Decompile(Expression)));
        }
        #endregion

        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression">Expression to decompile</param>
        public static Expression Decompile(this Expression Expression)
        {
            return DecompilerVisitor.Decompile(Expression);
        }

        /// <summary>
        /// Decompile a query that contains references to methods and properties that are marked with the Decompile attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Query"></param>
        /// <returns></returns>
        internal static IQueryable<T> PostDecompile<T>(this IQueryable<T> Query)
        {
            var Provider = Query.Provider;
            var Expression = Decompile(Query.Expression);
            return Provider.CreateQuery<T>(Expression);
        }

        /// <summary>
        /// Enable property decompilation for this query
        /// </summary>
        /// <returns>A query that expand property getters that implement the Expression.Execute pattern</returns>
        public static IQueryable<T> Decompile<T>(this IQueryable<T> Query)
        {
            var QueryType = Query.GetType();
            if (QueryType.IsGenericType && QueryType.GetGenericTypeDefinition() == typeof(DecompiledQuery<>))
            {
                //Si el query ya esta decompilado, devuelve la misma instancia
                return Query;
            }
            else
                return new DecompiledQuery<T>(Query);
        }

        /// <summary>
        /// Mark this query as server side, in such way that posterior Linq calls will be executed in memory
        /// </summary>
        public static IQueryable<T> ToClient<T>(this IQueryable<T> Query)
        {
            return new ClientSideQuery<T>(new ServerSideQuery<T>(Query));
        }

        /// <summary>
        /// Calls the generic Decompile method for a query with unknown element type
        /// </summary>
        /// <returns></returns>
        internal static IQueryable PostDecompile(IQueryable Query)
        {
            var dummy = new int[0].AsQueryable();

            var ex = (Expression<Func<IQueryable<int>>>)(() => dummy.PostDecompile());
            var Method = ((MethodCallExpression)ex.Body).Method;

            var GenericMethod = Method.GetGenericMethodDefinition().MakeGenericMethod(Query.ElementType);
            return (IQueryable)GenericMethod.Invoke(null, new object[] { Query });
        }


        /// <summary>
        /// In-place sort an observable collection using only the move method
        /// </summary>
        /// <typeparam name="TSource">Collection element type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="collection">The collection to sort</param>
        /// <param name="Selector">Key selector</param>
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> collection, Func<TSource, TKey> Selector)
        {
            var result = collection.Select((a, i) => new { a, i }).OrderBy(x => Selector(x.a)).ToList();
            for (int i = 0; i < result.Count; i++)
                collection.Move(result[i].i, i);
        }

        /// <summary>
        /// In-place sort an observable collection using only the move method
        /// </summary>
        /// <typeparam name="TSource">Collection element type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="collection">The collection to sort</param>
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> collection)
        {
            collection.Sort(x => x);
        }

        /// <summary>
        /// Get the given exception and all its neasted inner exceptions
        /// </summary>
        /// <param name="Ex">The original exception</param>
        /// <returns>A collection of all inner exceptions</returns>
        public static IEnumerable<Exception> GetAllExceptions(this Exception ex)
        {
            List<Exception> exceptions = new List<Exception>() { ex };

            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                exceptions.Add(currentEx);
            }
            return exceptions;
        }

        /// <summary>
        /// Devuelve una cadena con todos los elementos separados
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos</typeparam>
        /// <param name="items">Elementos</param>
        /// <param name="separator">Separador entre elementos</param>
        public static string SequenceToString<T>(this IEnumerable<T> items, string separator)
        {
            StringBuilder B = new StringBuilder();
            bool first = true;
            foreach (var It in items)
            {
                if (!first)
                    B.Append(separator);
                B.Append(It.ToString());
                first = false;
            }
            return B.ToString();
        }
    }
}
