using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    class MethodDecompiler
    {
        public static bool TryGetExpression(MethodInfo Method, out LambdaExpression Result)
        {
            var Instance = FormatterServices.GetUninitializedObject(Method.DeclaringType);
            try
            {
                Tonic.ExtensionMethods.ThrowExecuteExpression = true;
                Method.Invoke(Instance, new object[0]);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is Tonic.ExtensionMethods.ExpressionException)
                {
                    Result = ((Tonic.ExtensionMethods.ExpressionException)ex.InnerException).Value;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Result = null;
                return false;
            }
            finally
            {
                Tonic.ExtensionMethods.ThrowExecuteExpression = false;
            }
            Result = null;
            return false;
        }
    }
}
