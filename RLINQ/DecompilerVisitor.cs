using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressive;

namespace Tonic
{
    class DecompilerVisitor : ExpressionVisitor
    {
        public DecompilerVisitor()
        {
            this.Decompiler = ExpressiveEngine.GetDecompiler();
        }

        private Expressive.IDecompiler Decompiler;

        public static Expression Decompile(Expression Expr)
        {
            var Visitor = new DecompilerVisitor();
            var Ret = Visitor.Visit(Expr);
            return Ret;
        }

        private static bool IsComplexGetter(LambdaExpression Getter)
        {
            var Body = Getter.Body;
            var thisExpr = Getter.Parameters[0];

            if (Body is MemberExpression)
            {
                var aux = (MemberExpression)Body;
                if (aux.Expression == thisExpr)
                {
                    if (aux.Member is FieldInfo)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsExecuteExpression(LambdaExpression Getter, out LambdaExpression Ex)
        {
            var Body = Getter.Body;
            var thisExpr = Getter.Parameters[0];

            if (Body is MethodCallExpression)
            {
                return IsExecuteExpression((MethodCallExpression)Body, out Ex);
            }

            Ex = null;
            return false;
        }

        private static bool IsExecuteExpression(MethodCallExpression aux, out LambdaExpression Ex)
        {
            var ExecuteMethod = typeof(ExtensionMethods).GetMethod(nameof(ExtensionMethods.Execute));
            if (aux.Method.IsGenericMethod && aux.Method.GetGenericMethodDefinition() == ExecuteMethod)
            {
                var arg = aux.Arguments[0];
                var argCompile = Expression.Lambda(arg).Compile();
                var argExecute = (LambdaExpression)argCompile.DynamicInvoke();

                Ex = argExecute;
                return true;
            }

            Ex = null;
            return false;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            LambdaExpression ExecuteDecompile;

            if (IsExecuteExpression(node, out ExecuteDecompile))
            {
                var Decompile = ExecuteDecompile;
                var Expr = ReplaceVisitor.Replace(Decompile.Body, Decompile.Parameters[0], node.Arguments[1]);
                var Ret = DecompilerVisitor.Decompile(Expr);
                var ExecuteArg = node.Arguments[1];

                return Ret;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo)
            {
                var P = (PropertyInfo)node.Member;
                if (P.CanRead)
                {
                    LambdaExpression Decompile = null;

                    MethodInfo Method;
                    Method = P.GetMethod;
                    try
                    {
                        Decompile = Decompiler.Decompile(Method);
                    }
                    catch (Exception)
                    {
                    }

                    if (Decompile != null && IsComplexGetter(Decompile))
                    {
                        var thisExpr = Decompile.Parameters[0];
                        Expression Expr;
                        LambdaExpression ExecuteDecompile;
                        if (IsExecuteExpression(Decompile, out ExecuteDecompile))
                        {
                            Decompile = ExecuteDecompile;
                            Expr = ReplaceVisitor.Replace(Decompile.Body, Decompile.Parameters[0], node.Expression);
                        }
                        else
                        {

                            Expr = ReplaceVisitor.Replace(Decompile.Body, thisExpr, node.Expression);
                        }

                        Expression NeastedDecompile;
                        try
                        {
                            NeastedDecompile = DecompilerVisitor.Decompile(Expr);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException($"Can't decompile {Expr}");
                        }
                        return NeastedDecompile;
                    }
                }
            }

            return base.VisitMember(node);
        }
    }
}
