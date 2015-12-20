using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.ExpressionExecute
{
    /// <summary>
    /// Expression execute fluent API
    /// </summary>
    public class ExpressionFluent<T>
    {
        internal ExpressionFluent(T Instance)
        {
            this.Instance = Instance;
        }
        T Instance;

        /// <summary>
        /// Returns Expr with an injected parameter for this instance
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="ExprUsingThis">Expression using this instance</param>
        public Expression<Func<T, TResult>> Select<TResult>(Expression<Func<TResult>> ExprUsingThis)
        {
            var param = Expression.Parameter(typeof(T), "instance");
            Func<Expression, bool> Pred = ex => ex is ConstantExpression && object.Equals(((ConstantExpression)ex).Value, Instance);
            Func<Expression, Expression> Selector = ex => param;

            var Replace = new ReplaceVisitor(Pred, Selector).Visit(ExprUsingThis.Body);

            var Ret = Expression.Lambda<Func<T, TResult>>(Replace, param);
            return Ret;
        }
    }
}
