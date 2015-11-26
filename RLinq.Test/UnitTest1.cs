using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic;
using System.Linq;
namespace RLinq.Test
{
    [TestClass]
    public class UnitTest1
    {
        public class Artist
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }

            public string NombreCompleto
            {
                get
                {
                    return this.Execute(x => x.Nombre + " " + x.Apellido);
                }
            }

        }

        public class ArtistViewModel
        {
            public ArtistViewModel(Artist Model)
            {
                this.Model = Model;
            }

            public readonly Artist Model;
        }

        [TestMethod]
        public void RLinqFormat()
        {
            string Sql = "select \"Extent1\".\"nombre\" from \"public\".\"cliente\" AS \"Extent1\" ";
            var R = Tonic.RLinq.FormatSql(Sql);

        }

        [TestMethod]
        public void RLinqDecompileMin()
        {
            var items = new[]
            {
                new { A = 1, B = 2 },
                new { A = 3, B = 2}
            }.AsQueryable().Decompile();

            //Estas dos expresiones deben de ser equivalentes
            var q1 = items.Select(x => Math.Min(x.A, x.B));
            var q2 = items.Select(x => new[] { x.A, x.B }.Min());

            var e1 = q1.Expression.ToString();
            var e2 = q2.Expression.ToString();

            Assert.AreEqual(e1, e2);

            var r1 = q1.ToArray();
            var r2 = q2.ToArray();

            Assert.IsTrue(r1.SequenceEqual(r2));
        }

        [TestMethod]
        public void RLinqDecompileMax()
        {
            var items = new[]
            {
                new { A = 1, B = 2 },
                new { A = 3, B = 2}
            }.AsQueryable().Decompile();

            //Estas dos expresiones deben de ser equivalentes
            var q1 = items.Select(x => Math.Max(x.A, x.B));
            var q2 = items.Select(x => new[] { x.A, x.B }.Max());

            var e1 = q1.Expression.ToString();
            var e2 = q2.Expression.ToString();

            Assert.AreEqual(e1, e2);

            var r1 = q1.ToArray();
            var r2 = q2.ToArray();

            Assert.IsTrue(r1.SequenceEqual(r2));
        }
        


        [TestMethod]
        public void RLinqDecompile()
        {
            var items = (new[]
            {
                new Artist { Nombre = "Alejandra", Apellido = "Llanez" },
                new Artist { Nombre = "Rafael", Apellido = "Salguero" }
            }
            ).AsQueryable();

            var it2 = items.Decompile();



            var q1 = it2.Where(ar => ar.NombreCompleto == "Rafael Salguero");
            var q2 = it2.Where(ar => ar.Nombre + " " + ar.Apellido == "Rafael Salguero");


            var e1 = q1.Expression.ToString();
            var e2 = q2.Expression.ToString();

            Assert.AreEqual(e1, e2);

            var r1 = q1.ToArray();
            var r2 = q2.ToArray();

            Assert.IsTrue(r1.SequenceEqual(r2));

            //Soporte para IOrderedQueryable:
            //No debe de disparar excepciones:
            var r3 = q1.OrderBy(x => x.Nombre).ToArray();

            //Si el query ya esta decompilado, el método decompile no debe de hacer nada:
            var it3 = it2.Decompile();
            Assert.AreEqual(it3, it2);
        }
    }
}
