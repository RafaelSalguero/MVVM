using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.Composer
{
    internal interface IQueryComposerReference
    {
        IQueryComposer Composer
        {
            get;
        }
    }

    /// <summary>
    /// A query composer can apply an expression as a further transformation to another given IQueryable
    /// </summary>
    internal interface IQueryComposer
    {

        /// <summary>
        /// Returns an enumerator for the current query after its composition. This enumerator is warranted to be an instance of the generic IEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator ExecuteEnumerator();

        /// <summary>
        /// Returns the result of the given aggregator applied to the query after its composition
        /// </summary>
        /// <param name="Aggregator"></param>
        /// <returns></returns>
        TResult ExecuteAggregator<TResult>(Expression Aggregator);

        /// <summary>
        /// Create a query composer altered by the given expression
        /// </summary>
        /// <param name="alterExpression">The alter expression that contains references to this composer</param>
        /// <returns></returns>
        IQueryComposer Alter<NewT>(Expression alterExpression);
    }
}
