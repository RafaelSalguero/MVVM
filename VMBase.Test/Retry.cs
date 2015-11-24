using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.MVVM;

namespace VMBase.Test
{
    [TestClass]
    public class RetryTestClass
    {
        public class Fail
        {
            public int MaxCount = 5;
            public int Count = 0;
            public int Do()
            {
                Count++;
                if (Count > MaxCount)
                    return 123;
                else
                    throw new ApplicationException("Hola");
            }
        }

        [TestMethod]
        public void RetryTest()
        {
            var F = new Fail();
            var Result = Retry.Do(F.Do, 5);
            Assert.AreEqual(123, Result);

            //El metodo se ejecuto 6 veces, se reintento 5 veces
            Assert.AreEqual( 6, F.Count);

            //No se debe de reintentar mas de 4 veces:
            bool fail = false;
            F = new Fail();
            try
            {
                Retry.Do(F.Do, 4);
            }
            catch (Exception)
            {
                fail = true;
            }
            Assert.AreEqual(true, fail);
            Assert.AreEqual(5, F.Count);

            F = new Fail();
            var R = Retry.Do(F.Do, 1000);
            Assert.AreEqual(6, F.Count);
        }
    }
}
