using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.Implementation;

namespace Tonic.Patterns.PagedQuery
{
    /// <summary>
    /// Defines factory methods for creating paged queries
    /// </summary>
    public static class QueryFactory
    {
        /// <summary>
        /// Default page size
        /// </summary>
        public const int DefaultPageSize = 1000;

        /// <summary>
        /// Default page count
        /// </summary>
        public const int DefaultPageCount = 7;

        /// <summary>
        /// Sets whether this context query execute the non-generic GetEnumerator method as an item getter or as an immediate page enumerator
        /// </summary>
        /// <param name="Mode"></param>
        public static void SetImmediateEnumeratorMode(System.Collections.IEnumerable Query, bool Mode)
        {
            var Type = Query.GetType();
            if (Type.IsGenericType &&
                (
                Type.GetGenericTypeDefinition() == typeof(Implementation.AsyncOrderedContextQuery<,,>) ||
                Type.GetGenericTypeDefinition() == typeof(Implementation.AsyncContextQuery<,,>)
                )
                )
            {
                ((dynamic)Query).ImmediateEnumeratorMode = Mode;
            }
        }

        /// <summary>
        /// Create a paginated query encapsulating a given context. The context will be properly disposed if it implements IDisposable
        /// </summary>
        /// <typeparam name="TElement">Query element type</typeparam>
        /// <typeparam name="TContext">Query context type</typeparam>
        /// <param name="ContextCtor">Context creator</param>
        /// <param name="QueryCtor">Query creator</param>
        /// <param name="PageSize">Element count per page</param>
        /// <param name="PageCount">Number of pages in memory at any time</param>
        /// <returns></returns>
        public static IQueryable<TElement> CreateAsync<TElement, TContext>(Func<TContext> ContextCtor, Func<TContext, IQueryable<TElement>> QueryCtor, int PageSize, int PageCount, bool SupressContextDispose)
        {
            return new AsyncContextQuery<TContext, TElement, TElement>(ContextCtor, QueryCtor, PageSize, PageCount, SupressContextDispose);
        }

        /// <summary>
        /// Create a paginated query encapsulating a given context. The context will be properly disposed if it implements IDisposable
        /// </summary>
        /// <typeparam name="TElement">Query element type</typeparam>
        /// <typeparam name="TContext">Query context type</typeparam>
        /// <param name="ContextCtor">Context creator</param>
        /// <param name="QueryCtor">Query creator</param>
        /// <param name="PageSize">Element count per page</param>
        /// <param name="PageCount">Number of pages in memory at any time</param>
        /// <returns></returns>
        public static IQueryable<TElement> Create<TElement, TContext>(Func<TContext> ContextCtor, Func<TContext, IQueryable<TElement>> QueryCtor, int PageSize, int PageCount, bool SupressContextDispose)
        {
            return new ContextQuery<TContext, TElement, TElement>(ContextCtor, QueryCtor, PageSize, PageCount, SupressContextDispose);
        }

        /// <summary>
        /// Create a paginated query encapsulating a given context. The context will be properly disposed if it implements IDisposable. Default page size and page count will be used
        /// </summary>
        /// <typeparam name="TElement">Query element type</typeparam>
        /// <typeparam name="TContext">Query context type</typeparam>
        /// <param name="ContextCtor">Context creator</param>
        /// <param name="QueryCtor">Query creator</param>
        /// <returns></returns>
        public static IQueryable<TElement> Create<TElement, TContext>(Func<TContext> ContextCtor, Func<TContext, IQueryable<TElement>> QueryCtor, bool SupressContextDispose)
        {
            return Create(ContextCtor, QueryCtor, DefaultPageSize, DefaultPageCount, SupressContextDispose);
        }

        /// <summary>
        /// Create a paginated query from a given query
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <param name="PageSize">Element count per page</param>
        /// <param name="PageCount">Number of pages in memory at any time</param>
        /// <returns></returns>
        public static IQueryable<TElement> Create<TElement>(IQueryable<TElement> Query, int PageSize, int PageCount, bool SupressContextDispose)
        {
            return Create<TElement, object>(() => null, (c) => Query, PageSize, PageCount, SupressContextDispose);
        }

        /// <summary>
        /// Create a paginated query from a given query with the default page size and page count
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <returns></returns>
        public static IQueryable<TElement> Create<TElement>(IQueryable<TElement> Query)
        {
            return Create(Query, DefaultPageSize, DefaultPageCount);
        }

        /// <summary>
        /// Create a paginated query from a given query with the default page size and page count
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <returns></returns>
        public static IQueryable<TElement> CreateAsync<TElement>(IQueryable<TElement> Query)
        {
            return CreateAsync(Query, DefaultPageSize, DefaultPageCount);
        }

        /// <summary>
        /// Call the generic CreateAsync method when the element type is unknown
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <returns></returns>
        public static IQueryable CreateAsync(IQueryable Query)
        {
            Expression<Func<IQueryable>> expr = () => CreateAsync(new int[0].AsQueryable());
            var Method = ((MethodCallExpression)expr.Body).Method;
            var GenericMethod = Method.GetGenericMethodDefinition().MakeGenericMethod(Query.ElementType);
            return (IQueryable)GenericMethod.Invoke(null, new object[] { Query });
        }


        /// <summary>
        /// Call the generic Create method when the element type is unknown
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <returns></returns>
        public static IQueryable Create(IQueryable Query)
        {
            Expression<Func<IQueryable>> expr = () => Create(new int[0].AsQueryable());
            var Method = ((MethodCallExpression)expr.Body).Method;
            var GenericMethod = Method.GetGenericMethodDefinition().MakeGenericMethod(Query.ElementType);
            return (IQueryable)GenericMethod.Invoke(null, new object[] { Query });
        }



        /// <summary>
        /// Create a paginated query from a given query with the default page size and page count
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <returns></returns>
        public static IQueryable<TElement> CreateAsync<TContext, TElement>(TContext Context, Func<TContext, IQueryable<TElement>> Query)
        {
            return CreateAsync(() => Context, Query, DefaultPageSize, DefaultPageCount, true);
        }


        /// <summary>
        /// Create a paginated query from a given query
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <param name="PageSize">Element count per page</param>
        /// <param name="PageCount">Number of pages in memory at any time</param>
        /// <returns></returns>
        public static IQueryable<TElement> Create<TElement>(IEnumerable<TElement> Query, int PageSize, int PageCount)
        {
            return Create<TElement, object>(() => null, (c) => Query.AsQueryable(), PageSize, PageCount, true);
        }

        /// <summary>
        /// Create a paginated query from a given query
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <param name="PageSize">Element count per page</param>
        /// <param name="PageCount">Number of pages in memory at any time</param>
        /// <returns></returns>
        public static IQueryable<TElement> CreateAsync<TElement>(IEnumerable<TElement> Query, int PageSize, int PageCount)
        {
            return CreateAsync<TElement, object>(() => null, (c) => Query.AsQueryable(), PageSize, PageCount, true);
        }

        /// <

        /// <summary>
        /// Create a paginated query from a given query with the default page size and page count
        /// </summary>
        /// <typeparam name="TElement">Query element</typeparam>
        /// <param name="Query">The query to paginate</param>
        /// <returns></returns>
        public static IQueryable<TElement> Create<TElement>(IEnumerable<TElement> Query)
        {
            return Create(Query, DefaultPageSize, DefaultPageCount);
        }


    }
}
