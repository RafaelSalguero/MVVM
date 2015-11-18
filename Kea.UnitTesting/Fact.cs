using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kea.UnitTesting
{

    [Serializable]
    class UnitTestingException : ArgumentException
    {
        public UnitTestingException() { }
        public UnitTestingException(string message) : base(message + " " + Fact.GetFileLine()) { }
        public UnitTestingException(string message, Exception inner) : base(message + " " + Fact.GetFileLine(), inner) { }
        protected UnitTestingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
    /// <summary>
    /// Provee metodos para facilitar las pruebas unitarias
    /// </summary>
    public static class Fact
    {
        internal static string GetFileLine()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(ex, true);
                if (st == null) return null;

                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                var fileName = frame.GetFileName();
                return fileName + ", line: " + line;
            }
        }

        /// <summary>
        /// Establece que una expresion debe de ser true
        /// </summary>
        /// <param name="Actual"></param>
        public static void IsTrue(Expression<Func<bool>> Actual)
        {
            var Value = Actual.Compile()();
            if (!Value)
                throw new UnitTestingException($"IsTrue. {Actual.Body.ToString()}");
        }

        /// <summary>
        /// Establece que un valor debe de ser falso
        /// </summary>
        /// <param name="Actual"></param>
        public static void IsFalse(Expression<Func<bool>> Actual)
        {
            var Value = Actual.Compile()();
            if (Value)
                throw new UnitTestingException($"IsFalse. {Actual.Body.ToString()}");
        }

        private static string ExpressionToString(LambdaExpression Actual)
        {
            return "";
        }

        /// <summary>
        /// Establece que un valor debe de ser igual a otro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Expected"></param>
        /// <param name="Actual"></param>
        public static void AreEqual<T>(T Expected, Expression<Func<T>> Actual)
        {
            var F = Actual.Compile();
            var ActualValue = F();
            if (!object.Equals(Expected, Actual))
            {
                throw new UnitTestingException($"AreEqual. { ExpressionToString(Actual)}, Expected: {Expected}, Actual: {ActualValue} ");
            }
        }

        /// <summary>
        /// Establece que un valor debe de ser igual a otro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Expected"></param>
        /// <param name="Actual"></param>
        public static void AreEqual<T>(Expression<Func<T>> Expected, Expression<Func<T>> Actual)
        {
            var F = Actual.Compile();
            var ActualValue = F();
            if (!object.Equals(Expected, Actual))
            {
                throw new UnitTestingException($"AreEqual. {Expected.Body.ToString()}, {Actual.Body.ToString()}, Expected: {Expected}, Actual: {ActualValue} ");
            }
        }


    }
}
