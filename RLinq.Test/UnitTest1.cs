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
        public void RLinqDecompile()
        {
            var items = (new Artist[]
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
        }
    }
}
