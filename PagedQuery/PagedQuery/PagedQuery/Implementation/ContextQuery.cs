using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.Implementation.Base;
using Tonic.Patterns.PagedQuery.Pagination;
namespace Tonic.Patterns.PagedQuery.Implementation
{
    /// <summary>
    /// Synchronous implementation of the paged query
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    internal class ContextQuery<TContext, TIn, TOut> : BaseContextQuery<TContext, TIn, TOut>
    {
        public ContextQuery(Func<TContext> ContextCtor, Func<TContext, IQueryable<TIn>> QueryCtor, int pageSize, int pageCount, bool SupressContextDispose)
            : base(ContextCtor, QueryCtor, pageSize, pageCount, SupressContextDispose)
        {
        }



        #region Deferred query base
        /// <summary>
        /// Create a new context query
        /// </summary>
        /// <typeparam name="TNewOutput"></typeparam>
        /// <returns></returns>
        protected override Composer.DeferredQueryBase<TIn, TNewOutput> CreateNewOutputInstance<TNewOutput>(bool Ordered)
        {
            if (Ordered)
                return new OrderedContextQuery<TContext, TIn, TNewOutput>(ContextCtor, QueryCtor, pageSize, pageCount,SupressContextDispose );
            else
                return new ContextQuery<TContext, TIn, TNewOutput>(ContextCtor, QueryCtor, pageSize, pageCount, SupressContextDispose );
        }
        #endregion

        #region Count

        private int? count = null;
        /// <summary>
        /// Lazily initialized count
        /// </summary>
        public override int Count
        {
            get
            {
                if (count == null)
                    count = ((IQueryable<TOut>)this).Count();
                return count.Value;
            }
        }
        #endregion

        #region Items


        /// <summary>
        /// Gets by index an item from the context query, item retriving will trigger query execution of the given item isn't found on the current pages
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override TOut GetItem(int index)
        {
            var Page = GetPage(index);
            if (Page == null)
                return default(TOut);
            else
                return Page.Items[index - Page.Index];
        }
        #endregion
    }
}
