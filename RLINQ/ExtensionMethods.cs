using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    /// <summary>
    /// Provides extension methods for decompiling expression trees and IQueryables
    /// </summary>
    public static class ExtensionMethods
    {
        [ThreadStatic]
        private static Dictionary<LambdaExpression, Func<object, object>> expressionCache = new Dictionary<LambdaExpression, Func<object, object>>();

        /// <summary>
        /// Executes a one argument expression given the argument, the expression compilation is cached
        /// </summary>
        /// <typeparam name="TInput">The type of te input argument</typeparam>
        /// <typeparam name="TResult">Resulting type</typeparam>
        /// <param name="Expression">The expression to execute</param>
        /// <param name="Input">The input argument</param>
        /// <returns>The result of the expression execution</returns>
        public static TResult Execute<TInput, TResult>(this Expression<Func<TInput, TResult>> Expression, TInput Input)
        {
            Func<object, object> ret;
            if (!expressionCache.TryGetValue(Expression, out ret))
            {
                var Compile = Expression.Compile();
                ret = o => Compile((TInput)o);
                expressionCache.Add(Expression, ret);
            }

            return (TResult)ret(Input);
        }
        /// <summary>
        /// Decompile all expression that references methods or properties that are marked with the Decompile attribute
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        internal static Expression Decompile(this Expression Expression)
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


    }
}
