using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.ExVisitors
{
    /// <summary>
    /// Implement a visitor that matches all subexpression with a given predicate
    /// </summary>
    internal class ExpressionSearch : ExpressionVisitor
    {
        public ExpressionSearch(Func<Expression, bool> Predicate)
        {
            this.Predicate = Predicate;
        }

        private readonly Func<Expression, bool> Predicate;
        /// <summary>
        /// Holds search results
        /// </summary>
        public readonly HashSet<Expression> results = new HashSet<Expression>();

        public override Expression Visit(Expression node)
        {
            if (Predicate(node))
            {
                results.Add(node);
            }
            return base.Visit(node);
        }
    }
}
