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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //Convierte la expresion Math.Max(a,b) en new [] { a , b }.Max();
            //El Min de igual manera
            if (node.Method.DeclaringType == typeof(Math) && node.Method.Name == nameof(Math.Max))
            {
                var Type = node.Method.GetParameters()[0].ParameterType;
                var Array = Expression.NewArrayInit(Type, node.Arguments.Select(Visit).ToArray());
                var MaxMethod =
                     typeof(Enumerable).GetMethods()
                    .Where(x => x.Name == nameof(Enumerable.Max) && x.GetParameters().Length == 1 && !x.IsGenericMethod && x.GetParameters()[0].ParameterType == Type)
                    .FirstOrDefault()
                ??
                typeof(Enumerable).GetMethods()
                    .Where(x => x.Name == nameof(Enumerable.Max) && x.GetParameters().Length == 1 && x.IsGenericMethod)
                    .Single()
                    .MakeGenericMethod(Type);

                var MaxCall = Expression.Call(MaxMethod, Array);

                return MaxCall;
            }
            else if (node.Method.DeclaringType == typeof(Math) && node.Method.Name == nameof(Math.Min))
            {
                var Type = node.Method.GetParameters()[0].ParameterType;
                var Array = Expression.NewArrayInit(Type, node.Arguments.Select(Visit).ToArray());
                var MinMethod =
                     typeof(Enumerable).GetMethods()
                    .Where(x => x.Name == nameof(Enumerable.Min) && x.GetParameters().Length == 1 && !x.IsGenericMethod && x.GetParameters()[0].ParameterType == Type)
                    .FirstOrDefault()
                ??
                typeof(Enumerable).GetMethods()
                    .Where(x => x.Name == nameof(Enumerable.Min) && x.GetParameters().Length == 1 && x.IsGenericMethod)
                    .Single()
                    .MakeGenericMethod(Type);

                var MaxCall = Expression.Call(MinMethod, Array);

                return MaxCall;
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
