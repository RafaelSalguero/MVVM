using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.ExVisitors;

namespace Tonic.Patterns.PagedQuery.Composer
{
    /// <summary>
    /// Creates and executes deferred queries. This class warranties that doesn't hold an state thus, all instances of this type are equivalent
    /// </summary>
    internal class DeferredQueryProvider : IQueryProvider
    {
        #region Fields and properties
        //This class doesn't hold an state
        #endregion

        #region Methods
        private static Type GetAnyElementType(Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (typeof(Array).IsAssignableFrom(type))
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetInterfaces()
                                    .Where(t => t.IsGenericType &&
                                           t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                    .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return enumType ?? type;
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            //Search for a query composer on the given expression:
            var Search = new ExpressionSearch((x) => x is ConstantExpression && (((ConstantExpression)x).Value) is IQueryComposerReference);
            Search.Visit(expression);

            if (Search.results.Count == 0)
                throw new ArgumentException("No DeferredQuery was found on the given query expression tree");
            if (Search.results.Count > 1)
                throw new ArgumentException("Only single deferred query per query expression tree are supported");

            //Gets the composer:
            var Composer = ((IQueryComposerReference)((ConstantExpression)Search.results.First()).Value).Composer;

            var R = (IQueryable<TElement>)Composer.Alter<TElement>(expression);
            return R;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            throw new NotSupportedException();
            //Type elementType = GetAnyElementType(expression.Type);
            //try
            //{
            //    return (IQueryable)Activator.CreateInstance(typeof(DeferredQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            //}
            //catch (System.Reflection.TargetInvocationException tie)
            //{
            //    throw tie.InnerException;
            //}
        }

        private class DummyEnumerator<T> : IEnumerable<T>
        {
            public DummyEnumerator(IEnumerator<T> enumerator)
            {
                this.enumerator = enumerator;
            }
            public DummyEnumerator(IEnumerator enumerator)
                : this((IEnumerator<T>)enumerator)
            { }

            private readonly IEnumerator<T> enumerator;
            public IEnumerator<T> GetEnumerator()
            {
                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            //Search for a query composer on the given expression:
            var Search = new ExpressionSearch((x) => x is ConstantExpression && (((ConstantExpression)x).Value) is IQueryComposerReference);
            Search.Visit(expression);

            if (Search.results.Count == 0)
                throw new ArgumentException("No DeferredQuery was found on the given query expression tree");
            if (Search.results.Count > 1)
                throw new ArgumentException("Only single deferred query per query expression tree are supported");

            //Gets the composer:
            var Composer = ((IQueryComposerReference)((ConstantExpression)Search.results.First()).Value).Composer;

            if (expression.NodeType == ExpressionType.Constant)
            {
                //The execution results on another query
                //executing the query thus will result on enumerations:

                var ResultType = GetAnyElementType(((IQueryable)Composer).ElementType);
                var ElemType = GetAnyElementType(typeof(TResult));

                if (ResultType != ElemType)
                    throw new ArgumentOutOfRangeException("expression", "The requested element type is different from the resulting element type");
                var EnumType = typeof(DummyEnumerator<>).MakeGenericType(ElemType);
                var result = Activator.CreateInstance(EnumType, Composer.ExecuteEnumerator());


                return (TResult)result;
            }
            else
            {
                //The execution contains agregate functions:
                //this functions are passed as-is to the composed query and then the query is executed

                return Composer.ExecuteAggregator<TResult>(expression);
            }
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return Execute<object>(expression);
        }
        #endregion
    }
}
