using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.ExVisitors;
using System.Reflection;
namespace Tonic.Patterns.PagedQuery.Composer
{
    /// <summary>
    /// Base class for all deferred queries. This and all child classes should be immutable
    /// </summary>
    /// <typeparam name="TOutput">Element type of the resulting query</typeparam>
    /// <typeparam name="TInput">Element type of the input original query</typeparam>
    internal abstract class DeferredQueryBase<TInput, TOutput> : IQueryable<TOutput>, IQueryComposer
    {
        #region Constructors
        protected DeferredQueryBase()
        {
            this.inputExpression = Expression.Constant(new QueryPlaceholder<TInput>(this));
            this.expression = this.inputExpression;
        }
        #endregion

        #region Properties
        private Expression compositingExpression;

        /// <summary>
        /// Expression that will be applied to the resulting query
        /// </summary>
        public Expression CompositingExpression
        {
            get
            {
                if (compositingExpression == null)
                    compositingExpression = Expression;
                return compositingExpression;
            }
        }
        #endregion


        #region Immutable compositing

        /// <summary>
        /// Return the resulting expression for the compositing of the original query
        /// </summary>
        /// <returns></returns>
        protected Expression ComposeEnumerator(Expression QueryExpression)
        {
            var V = new ReplaceVisitor(this.InputExpression, QueryExpression);
            var Ret = V.Visit(CompositingExpression);

            return Ret;
        }

        /// <summary>
        /// Return the resulting expression for the compositing of the original query with the current compositing expression and an aditional aggregator
        /// </summary>
        /// <returns></returns>
        protected Expression ComposeAggregator(Expression QueryExpression, Expression Aggregator)
        {
            return new ReplaceVisitor(this.Expression, ComposeEnumerator(QueryExpression)).Visit(Aggregator);
        }

        /// <summary>
        /// Create a clone of this deferred query
        /// </summary>
        /// <typeparam name="TNewOutput"></typeparam>
        /// <returns></returns>
        protected abstract DeferredQueryBase<TInput, TNewOutput> CreateNewOutputInstance<TNewOutput>(bool Ordered);

        /// <summary>
        /// Clone this object. Set ordered to true to return the ordereed queryable implementation version
        /// </summary>
        /// <returns></returns>
        private DeferredQueryBase<TInput, TNewOutput> Clone<TNewOutput>(bool Ordered)
        {
            //Create a new instance of this type:
            var Instance = CreateNewOutputInstance<TNewOutput>(Ordered);
            var Type = GetType();
            var InstanceType = Instance.GetType();

            //Copy all fields, excluding the expression and the input expression:
            var Fields = Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var F in Fields)
            {
                if (!F.GetCustomAttributes<NonSerializedAttribute>(true).Any())
                {
                    var InstanceField = InstanceType.GetField(F.Name, BindingFlags.Public | BindingFlags.Instance);
                    InstanceField.SetValue(Instance, F.GetValue(this));
                }
            }

            //Set the expression to a new constant of TOutput:
            Instance.expression = Expression.Constant(new QueryPlaceholder<TNewOutput>(Instance));

            //Return the new instance:
            return Instance;
        }
        #endregion


        #region IQueryComposer
        public abstract System.Collections.IEnumerator ExecuteEnumerator();
        public abstract TResult ExecuteAggregator<TResult>(System.Linq.Expressions.Expression Aggregator);

        /// <summary>
        /// Create a new deferred query with a composer expression and composer value applied to this query
        /// </summary>
        /// <param name="alterExpression">The alter expression that contains references to this composer</param>
        /// <returns></returns>
        IQueryComposer IQueryComposer.Alter<NewT>(Expression alterExpression)
        {
            bool Ordered = typeof(IOrderedQueryable).IsAssignableFrom(alterExpression.Type);

            //Compose both the composer and the given expression:
            var Rep = new ReplaceVisitor(this.Expression, this.CompositingExpression);
            var FullComposer = Rep.Visit(alterExpression);

            var Ret = Clone<NewT>(Ordered);
            Ret.compositingExpression = FullComposer;


            return Ret;
        }
        #endregion

        #region IQueryable
        public IEnumerator<TOutput> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<TOutput>>(Expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(TOutput); }
        }


        /// <summary>
        /// public enables auto clonning.
        /// </summary>
        public Expression inputExpression;
        /// <summary>
        /// Input expression is used as a placeholder on the composed expression for the deferred original query
        /// </summary>
        private Expression InputExpression
        {
            get
            {
                return inputExpression;
            }
        }
        private Expression expression;
        /// <summary>
        /// Holds an expression that repesents the query output of this query base
        /// </summary>
        public Expression Expression
        {
            get
            {
                return expression;
            }
        }


        /// <summary>
        /// Because the derferred query provider doesn't hold an state, each instance can share the same query provider
        /// </summary>
        private static DeferredQueryProvider provider;

        /// <summary>
        /// Deferred query provider
        /// </summary>
        public IQueryProvider Provider
        {
            get
            {
                if (provider == null)
                    provider = new DeferredQueryProvider();
                return provider;
            }
        }
        #endregion
    }
}
