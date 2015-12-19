using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kea.Progress.TEst
{
    [TestClass]
    public class UnitTest1
    {
        public class TestProgress : IProgress<double>
        {
            public double value;
            public void Report(double value)
            {
                this.value = value;
            }
        }

        void eq(double a, double b)
        {
            var dif = Math.Abs(a - b);
            if (dif > 0.0001)
            {
                Assert.Fail($"Expected: {a}, Actual: {b}");
            }
        }


        [TestMethod]
        public void AsyncProgressTest()
        {
            var Prog = new TestProgress();
            var P = Prog.Child(  2);

            //Agrega dos tareas que su progreso sera intercalado:
            var T1 = P.Child(2);
            var T2 = P.Child(2);

            eq(0.0, Prog.value);

            T1.Step();
            eq(1.0 / 4.0, Prog.value);

            T1.Step();
            eq(2.0 / 4.0, Prog.value);

            //Termina la tarea P:
            P.Step();
            eq(1.0, Prog.value);
        }

        [TestMethod]
        public void ProgressTest()
        {
            var Prog = new TestProgress();
            //Prueba un progreso sin hijos:
            var P = Prog.Child(0, 3);

            Assert.AreEqual(0.0, Prog.value);

            P.Step();
            Assert.AreEqual(1.0 / 3.0, Prog.value);

            P.Step();
            Assert.AreEqual(2.0 / 3.0, Prog.value);

            P.Step();
            Assert.AreEqual(3.0 / 3.0, Prog.value);

            //Prueba un progreso con un hijo:
            P = Prog.Child(0, 3);

            var P1 = P.Child(2);
            P1.Step();

            Assert.AreEqual(1.0 / 6.0, Prog.value);
            //Finaliza la subtarea 1
            P1.Step();

            Assert.AreEqual(2.0 / 6.0, Prog.value);

            //El progreso de la subtarea 1 es igual:
            P.Report(1);
            Assert.AreEqual(1.0 / 3.0, Prog.value);

            //Crea otra subtarea, ahora con 4 subpasos:
            P1 = P.Child(4);
            P1.Step();

            eq((1.0 + (1.0 / 4.0)) / 3.0, Prog.value);

            P1.Step();
            eq((1.0 + (2.0 / 4.0)) / 3.0, Prog.value);

            P1.Step();
            eq((1.0 + (3.0 / 4.0)) / 3.0, Prog.value);

            //Finaliza la subtarea 4:
            P1.Step();
            eq(2.0 / 3.0, Prog.value);
            P.Report(2);
            eq(2.0 / 3.0, Prog.value);

            //Crea una subtarea con 2 pasos.
            P1 = P.Child(2);
            P1.Step();
            eq((2.0 + (1.0 / 2.0)) / 3.0, Prog.value);

            //Crea una sub subtarea con 3 pasos:
            var P2 = P1.Child(3);

            P2.Step();
            eq((2.0 + ((1.0 + (1.0 / 3.0)) / 2.0)) / 3.0, Prog.value);

            P2.Step();
            eq((2.0 + ((1.0 + (2.0 / 3.0)) / 2.0)) / 3.0, Prog.value);

            P2.Step();
            eq((2.0 + ((1.0 + (3.0 / 3.0)) / 2.0)) / 3.0, Prog.value);
            eq((2.0 + ((2.0) / 2.0)) / 3.0, Prog.value);
            eq(3.0 / 3.0, Prog.value);

        }
    }
}
