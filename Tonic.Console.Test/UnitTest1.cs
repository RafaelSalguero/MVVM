using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tonic.Console.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var s = "hola \"esto es una cadena, de texto\" rafa, esto es, una, linea de texto";
            var split = Tonic.Console.WordSplitter.SplitLines(s);

            ConsoleHelper C = new ConsoleHelper();
            C.Execute("\"hola rafa, como andas\", dup");

            Assert.AreEqual("hola rafa, como andas", C.Stack.Pop());
            Assert.AreEqual("hola rafa, como andas", C.Stack.Pop());

            C.Execute("(\"Bofan\")");


            C.Stack.Pop();
            Assert.AreEqual("Bofan", C.Stack.Pop());
            C.Stack.Pop();

        }
    }
}
