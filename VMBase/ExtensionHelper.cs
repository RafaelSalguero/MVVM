using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    class ExtensionHelper
    {
        /// <summary>
        /// Gets the property name for a property expression
        /// </summary>
        /// <returns>The name of the property</returns>
        public static PropertyInfo GetPropertyInfo(LambdaExpression Property)
        {
            var Argument = Property.Parameters[0];


            MemberExpression Body = Property.Body as MemberExpression;
            //The property has a cast to object:
            if ((Property.Body as UnaryExpression)?.NodeType == ExpressionType.Convert)
            {
                Body = ((UnaryExpression)Property.Body).Operand as MemberExpression;
            }

            if (Body == null)
                throw new ArgumentException($"The body {Body} is not a MemberExpression");

            var MemberExpression = Body;

            if (MemberExpression.Expression != Argument)
                throw new ArgumentException("Bindings to neasted properties are not supported");

            var Member = MemberExpression.Member;
            if (!(Member is PropertyInfo))
                throw new ArgumentException("Only property members are supported");

            return (PropertyInfo)Member;
        }
    }
}
