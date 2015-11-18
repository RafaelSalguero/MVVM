using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kea.UnitTesting.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void KeaUnitTest()
        {
            int valor = 2;
            Fact.AreEqual(0, () => valor);
        }
    }
}
