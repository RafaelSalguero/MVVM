using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    /// <summary>
    /// Replace a constant expression with another expression
    /// </summary>
    class ReplaceVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Create a constant replace expression visitor
        /// </summary>
        public ReplaceVisitor(Expression ex, Expression ReplaceWith)
        {
            this.ex = ex;
            this.replaceWith = ReplaceWith;
        }


        public static Expression Replace(Expression Expression, Expression Find, Expression ReplaceWith)
        {
            var V = new ReplaceVisitor(Find, ReplaceWith);
            return V.Visit(Expression);
        }


        private readonly Expression ex;
        private readonly Expression replaceWith;
        public bool Any = false;
        public override Expression Visit(Expression node)
        {
            if (ex.Equals(node))
            {
                Any = true;
                return replaceWith;
            }
            else
                return base.Visit(node);
        }
    }
}
