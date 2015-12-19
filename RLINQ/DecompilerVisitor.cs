using System;
using System.Collections.Generic;
using System.Data.Entity;
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

            //Aplica el expression expander:
            Ret = new ExpressionExpander.ExpressionExpander().Visit(Ret);
            return Ret;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            //Convert DateTime + TimeSpan to
            //DbFunctions.AddMilliseconds ( DateTime, DbFunctions.DiffMilliseconds(TimeSpan.Zero, TimeSpan));
            if (
                node.NodeType == ExpressionType.Add &&
                (node.Left.Type == typeof(DateTime) || node.Left.Type == typeof(DateTime?)) &&
                (node.Right.Type == typeof(TimeSpan) || node.Right.Type == typeof(TimeSpan?))
                )
            {
                var AddMethod = typeof(DbFunctions).GetMethods()
                    .Where(x => x.Name == nameof(DbFunctions.AddMilliseconds))
                    .Where(x => x.GetParameters()[0].ParameterType == typeof(DateTime?))
                    .Single();

                var DiffMethod = typeof(DbFunctions).GetMethods()
                    .Where(x => x.Name == nameof(DbFunctions.DiffMilliseconds))
                    .Where(x => x.GetParameters()[0].ParameterType == typeof(TimeSpan?))
                    .Single();

                var DateOrNull = node.Left;
                var TimeOrNull = node.Right;

                //Convierte a nullable si los valores no son nullables,
                //pues esta conversion aunque sea realizada automaticamente por el
                //compilador no es realizado automaticamente por las expresiones
                Expression Date, Time;
                if (DateOrNull.Type == typeof(DateTime))
                    Date = Expression.Convert(DateOrNull, typeof(DateTime?));
                else
                    Date = DateOrNull;

                if (TimeOrNull.Type == typeof(TimeSpan))
                    Time = Expression.Convert(TimeOrNull, typeof(TimeSpan?));
                else
                    Time = TimeOrNull;

                var Zero = Expression.Convert(Expression.Field(null, typeof(TimeSpan), nameof(TimeSpan.Zero)), typeof(TimeSpan?));

                var DiffCall = Expression.Call(DiffMethod, Zero, Time);

                var AddCall = Expression.Call(AddMethod, Date, DiffCall);
                //Convierte de vuelta a un tipo no nullable si el resultado del nodo original
                //no era nulable
                if (node.Type == typeof(DateTime))
                {
                    return Expression.Property(AddCall, typeof(DateTime?).GetProperty(nameof(Nullable<double>.Value)));
                }
                else
                    return AddCall;

            }
            return base.VisitBinary(node);
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
