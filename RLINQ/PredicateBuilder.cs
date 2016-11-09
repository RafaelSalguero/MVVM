using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    /// <summary>
    /// Predicate builder
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Returns Expr
        /// </summary>
        public static Expression<Func<T, bool>> New<T>(Expression<Func<T, bool>> Expr)
        {
            return Expr;
        }

        /// <summary>
        /// Returns an expression that always returns true
        /// </summary>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }

        /// <summary>
        /// Returns an expression that always returns false
        /// </summary>
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        /// <summary>
        /// Returns an expression that returns expr1 || expr2
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var FirstArg = expr1.Parameters[0];
            var SecondArg = expr2.Parameters[0];

            var Replace = new ReplaceVisitor(SecondArg, FirstArg);
            var TEx2 = (Expression<Func<T, bool>>)Replace.Visit(expr2);

            var R = Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, TEx2.Body), FirstArg);
            return R;
        }


        /// <summary>
        /// Convers a predicate statement onto a select statement
        /// </summary>
        /// <typeparam name="TIn">Predicate parameter type</typeparam>
        /// <typeparam name="TOut">Selected type</typeparam>
        /// <param name="expr1"></param>
        /// <param name="Selector"></param>
        /// <returns></returns>
        public static Expression<Func<TIn, TOut>> Select<TIn, TOut>(this Expression<Func<TIn, bool>> expr1, Expression<Func<TIn, TOut>> Selector)
        {
            var FirstArg = expr1.Parameters[0];

            var SecondArg = Expression.Parameter(typeof(TIn));

            var TSelector = (Expression<Func<TIn, TOut>>)(new ReplaceVisitor(Selector.Parameters[0], SecondArg)).Visit(Selector);

            var R = new ReplaceVisitor(Selector.Parameters[0], TSelector.Body);
            var SelectedExpr = R.Visit(TSelector.Body);

            return Expression.Lambda<Func<TIn, TOut>>(SelectedExpr, SecondArg);
        }

        /// <summary>
        /// Create a contains statement made of a collection of OR operations.
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> In<T, TItem>(this Expression<Func<T, TItem>> expr1, IEnumerable<TItem> Items)
        {
            if (Items.Any())
            {
                var FirstArg = expr1.Parameters[0];
                var body = expr1.Body;

                Expression Result = Expression.Equal(body, Expression.Constant(Items.First()));
                foreach (var It in Items.Skip(1))
                    Result = Expression.Or(Result, Expression.Equal(body, Expression.Constant(It)));

                return Expression.Lambda<Func<T, bool>>(Result, FirstArg);
            }
            else
            {
                return False<T>();
            }
        }

        /// <summary>
        /// Returns an expression that returns expr1 &amp;&amp; expr2
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var FirstArg = expr1.Parameters[0];
            var SecondArg = expr2.Parameters[0];

            var Replace = new ReplaceVisitor(SecondArg, FirstArg);
            var TEx2 = (Expression<Func<T, bool>>)Replace.Visit(expr2);

            var R = Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, TEx2.Body), FirstArg);
            return R;
        }


        static Expression PredicateEqualBody(Type ElementType, IEnumerable<Tuple<string, object>> PropertyValues, ParameterExpression Instance)
        {

            Expression REx = null;
            foreach (var P in PropertyValues)
            {
                var Property = Expression.Property(Instance, P.Item1);

                var PropertyType = ((PropertyInfo)Property.Member).PropertyType;
                Expression Value = Expression.Constant(P.Item2);

                if (PropertyType.IsNullable() && Value == null || !Value.GetType().IsNullable())
                {
                    Value = Expression.Convert(Value, PropertyType);
                }

                var Eq = Expression.Equal(Property, Value);
                if (REx == null)
                    REx = Eq;
                else
                    REx = Expression.And(REx, Eq);
            }

            return REx;
        }
        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the RLinq.Where method
        /// </summary>
        /// <param name="ElementType"></param>
        /// <param name="PropertyValues">A collection that matches the property and the values to test</param>
        /// <returns></returns>
        public static Expression PredicateEqual(Type ElementType, IEnumerable<Tuple<string, object>> PropertyValues)
        {
            var Instance = Expression.Parameter(ElementType);
            var REx = PredicateEqualBody(ElementType, PropertyValues, Instance);
            var Lambda = Expression.Lambda(REx, Instance);
            return Lambda;
        }

        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the native Where method
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> PredicateEqual<T>(IEnumerable<Tuple<string, object>> PropertyValues)
        {
            var ElementType = typeof(T);
            var Instance = Expression.Parameter(ElementType);
            var REx = PredicateEqualBody(ElementType, PropertyValues, Instance);
            var Lambda = Expression.Lambda<Func<T, bool>>(REx, Instance);
            return Lambda;
        }


        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the RLinq.Where method
        /// </summary>
        public static Expression PredicateEqual(Type ElementType, IEnumerable<string> PropertyNames, IEnumerable<object> PropertyValues)
        {
            return PredicateEqual(ElementType, PropertyNames.Zip(PropertyValues, (a, b) => Tuple.Create(a, b)));
        }

        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the RLinq.Where method
        /// </summary>
        public static Expression<Func<T, bool>> PredicateEqual<T>(IEnumerable<string> PropertyNames, IEnumerable<object> PropertyValues)
        {
            return PredicateEqual<T>(PropertyNames.Zip(PropertyValues, (a, b) => Tuple.Create(a, b)));
        }

        /// <summary>
        /// Returns an expression that test a property equality to a given value
        /// </summary>
        public static Expression PredicateEqual(Type ElementType, string PropertyName, object PropertyValue)
        {
            return PredicateEqual(ElementType, new[] { PropertyName }, new[] { PropertyValue });
        }

        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the RLinq.Where method
        /// </summary>
        public static Expression<Func<T, bool>> PredicateEqual<T>(string PropertyName, object PropertyValue)
        {
            return PredicateEqual<T>(new[] { PropertyName }, new[] { PropertyValue });
        }

    }
}
