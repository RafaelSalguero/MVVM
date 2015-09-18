using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Tonic
{
    class DecompilerVisitor : ExpressionVisitor
    {
        public DecompilerVisitor()
        { }

        public static Expression Decompile(Expression Expr)
        {
            var Visitor = new DecompilerVisitor();
            var Ret = Visitor.Visit(Expr);
            return Ret;
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

                    if (MethodDecompiler.TryGetExpression(Method, out Decompile))
                    {
                        var thisExpr = Decompile.Parameters[0];
                        Expression Expr;
                        Expr = ReplaceVisitor.Replace(Decompile.Body, thisExpr, node.Expression);

                        Expression NeastedDecompile;
                        NeastedDecompile = DecompilerVisitor.Decompile(Expr);
                        return NeastedDecompile;
                    }
                }
            }

            return base.VisitMember(node);
        }
    }
}
