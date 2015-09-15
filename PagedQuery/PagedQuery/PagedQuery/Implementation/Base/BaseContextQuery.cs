using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.Pagination;

namespace Tonic.Patterns.PagedQuery.Implementation.Base
{
    /// <summary>
    /// A deferred query that its creator depends on a context, that will also be created in place when enumerated. ContextQueries doesn't preserve integrity if the underlying data source changes
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    internal abstract partial class BaseContextQuery<TContext, TIn, TOut> : Composer.DeferredQueryBase<TIn, TOut>, IList<TOut>, IList, IPaginator<TOut>
    {
        #region Constructors
        /// <summary>
        /// Create a new context query
        /// </summary>
        public BaseContextQuery(Func<TContext> ContextCtor, Func<TContext, IQueryable<TIn>> QueryCtor, int pageSize, int pageCount, bool SupressContextDispose)
        {
            this.SupressContextDispose = SupressContextDispose;
            this.ContextCtor = ContextCtor;
            this.QueryCtor = QueryCtor;
            this.pageSize = pageSize;
            this.pageCount = pageCount;
            this.paginator = new Pagination.PageCache<TOut>(pageSize, pageCount);
        }
        #endregion

        #region Fields
        protected readonly bool SupressContextDispose;
        protected readonly int pageSize, pageCount;

        protected readonly Func<TContext> ContextCtor;
        protected readonly Func<TContext, IQueryable<TIn>> QueryCtor;

        /// <summary>
        /// Pages are lazily initialized
        /// </summary>
        protected Pagination.PageCache<TOut> paginator = null;
        #endregion

        #region Deferred query base


        /// <summary>
        /// Execute an immediate aggregator function over this query
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Aggregator"></param>
        /// <returns></returns>
        public override TResult ExecuteAggregator<TResult>(System.Linq.Expressions.Expression Aggregator)
        {
            Func<TContext, TResult> execute = (C) =>
                {
                    Stopwatch st = new Stopwatch();

                    //Create the query:
                    var Query = QueryCtor(C);

                    //Compose the aggregator function:
                    var Composed = ComposeAggregator(Query.Expression, Aggregator);


                    var ret = Query.Provider.Execute<TResult>(Composed);

                    return ret;
                };

            //Create and dispose the context correctly:
            var context = ContextCtor();

            var dispContext = context as IDisposable;
            if (SupressContextDispose) dispContext = null;

            if (dispContext != null)
                using (dispContext)
                { return execute(context); }
            else
                return execute(context);
        }


        /// <summary>
        /// Execute query enumerator
        /// </summary>
        /// <returns></returns>
        public override System.Collections.IEnumerator ExecuteEnumerator()
        {
            return ((IEnumerable<TOut>)this).GetEnumerator();
        }
        #endregion
        #region Pagination
        Page<TOut> IPaginator<TOut>.GetPage(int index)
        {
            return GetPage(index);
        }

        /// <summary>
        /// The last accesed page is saved for performance pruposes (Its highly likely that subsequent access would be consecutive)
        /// </summary>
        private Page<TOut> lastPage;


        /// <summary>
        /// Gets the page containing the givin index, if the index is not cached, results null
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Page<TOut> GetExistingPage(int index)
        {
            if (paginator == null)
                return null;
            Page<TOut> existingPage;
            if (lastPage != null && lastPage.ContainsIndex(index))
            {
                //last page hit
                return lastPage;
            }
            else if (paginator.TryGetPage(index, out existingPage))
            {
                return existingPage;
            }
            return null;
        }

        internal Page<TOut> GetPage(int index)
        {
            bool dummy;
            return GetPage(index, out dummy);
        }
        /// <summary>
        /// Gets a page containing the given index, if the index is not cached, a new page is filled with query data
        /// </summary>
        /// <returns></returns>
        internal Page<TOut> GetPage(int index, out bool CacheHit)
        {
            //Lazily initialize page count
            if (paginator == null)
                paginator = new Pagination.PageCache<TOut>(pageSize, pageCount);

            Page<TOut> existingPage;
            existingPage = GetExistingPage(index);

            CacheHit = true;
            if (existingPage == null)
            {
                CacheHit = false;
                //An existing page wasn't found, fill a new page with query data:

                Func<Page<TOut>> FillPage = () =>
                {
                    //Get the page index:
                    int NewPageIndex = (index / pageSize) * pageSize;

                    //Fill the page:
                    var NewPage = paginator.AddPage(NewPageIndex);
                    NewPage.Count = ImmediateExecuteEnumerator((q) => q.Skip(NewPageIndex).Take(pageSize), NewPage.Items);

                    return NewPage;
                };

                existingPage = FillPage();
            }
            lastPage = existingPage;

            return lastPage;
        }

        /// <summary>
        /// Compose the resulting query after the context was created
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public IQueryable<TOut> ComposeQuery(TContext C)
        {
            //Create the query:
            var OriginalQuery = QueryCtor(C);

            //Compose the query:
            var ComposedExpression = ComposeEnumerator(OriginalQuery.Expression);

            //Create the query with the underlying query provider:
            return OriginalQuery.Provider.CreateQuery<TOut>(ComposedExpression);
        }


        /// <summary>
        /// Execute a query and save the query results onto an array
        /// </summary>
        /// <returns></returns>
        public int ImmediateExecuteEnumerator(Func<IQueryable<TOut>, IQueryable<TOut>> QueryTransform, TOut[] output)
        {

            var context = ContextCtor();
            var dispContext = context as IDisposable;
            if (SupressContextDispose) dispContext = null;
            if (dispContext != null)
                using (dispContext)
                {
                    int index = 0;
                    var Q = QueryTransform(ComposeQuery(context));
                    foreach (var el in Q)
                    {
                        output[index] = el;
                        index++;
                    }
                    return index;
                }
            else
            {
                int index = 0;
                var Q = QueryTransform(ComposeQuery(context));
                foreach (var el in Q)
                {
                    output[index] = el;
                    index++;
                }
                return index;
            }

        }



        #endregion


    }
}
