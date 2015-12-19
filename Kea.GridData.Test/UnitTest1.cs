using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
namespace Kea.GridData.Test
{
    [TestClass]
    public class KeaGridDataTest
    {
        public class Credito
        {
            public string Cliente { get; set; }
            [ColumnList(nameof(Saldo.Title), false)]
            public Saldo[] Saldos { get; set; }
        }

        public class Saldo
        {
            [Ignore]
            public string Title { get; set; }
            public int Value { get; set; }
        }


        [TestMethod]
        public void DynamicColumnTest()
        {
            var Set = new[]
            {
                new Credito { Cliente ="Rafael Salguero", Saldos = new []
                {
                    new Saldo { Title ="1 a 7", Value = 1000},
                    new Saldo { Title ="8 a 14", Value = 2000},
                } },

                 new Credito { Cliente ="Alejandra Llanez", Saldos = new []
                {
                    new Saldo { Title ="1 a 7", Value = 500},
                    new Saldo { Title ="8 a 14", Value = 800},
                } },
            };

            var Columns = DataColumn.FromData(Set).ToList();
            Assert.AreEqual("Cliente", Columns[0].FriendlyName);
            Assert.AreEqual("1 a 7", Columns[1].FriendlyName);
            Assert.AreEqual("8 a 14", Columns[2].FriendlyName);

            var Value = Columns[2].PropertyGetter(Set[0]);

            var Grid = Data.ToGrid(Set, Columns);

            //Fila 1:
            Assert.AreEqual("Rafael Salguero", Grid[0, 0]);
            Assert.AreEqual(1000, Grid[0, 1]);
            Assert.AreEqual(2000, Grid[0, 2]);

            //Fila 2:
            Assert.AreEqual("Alejandra Llanez", Grid[1, 0]);
            Assert.AreEqual(500, Grid[1, 1]);
            Assert.AreEqual(800, Grid[1, 2]);
        }
    }
}
