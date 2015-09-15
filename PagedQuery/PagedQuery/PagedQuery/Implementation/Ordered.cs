using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.Implementation
{
    internal class OrderedContextQuery<TContext, TIn, TOut> : ContextQuery<TContext, TIn, TOut>, IOrderedQueryable<TOut>
    {
        public OrderedContextQuery(Func<TContext> ContextCtor, Func<TContext, IQueryable<TIn>> QueryCtor, int pageSize, int pageCount, bool SupressContextDispose )
            : base(ContextCtor, QueryCtor, pageSize, pageCount,  SupressContextDispose )
        {
        }
    }

    internal class AsyncOrderedContextQuery<TContext, TIn, TOut> : AsyncContextQuery<TContext, TIn, TOut>, IOrderedQueryable<TOut>
    {
        public AsyncOrderedContextQuery(Func<TContext> ContextCtor, Func<TContext, IQueryable<TIn>> QueryCtor, int pageSize, int pageCount, bool SupressContextDispose)
            : base(ContextCtor, QueryCtor, pageSize, pageCount,  SupressContextDispose )
        {
        }
    }
}
