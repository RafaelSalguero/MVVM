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

            public static Expression<Func<Artist, string>> exNombreCompleto = x => x.Nombre + " " + x.Apellido;
            public string NombreCompleto
            {
                get
                {
                    return ((Expression<Func<Artist, string>>)(x => x.Nombre)).Execute(this);
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
            var q3 = it2.Where(ar => Artist.exNombreCompleto.Execute(ar) == "Rafael Salguero");

            var e1 = q1.Expression.ToString();
            var e2 = q2.Expression.ToString();
            var e3 = q3.Expression.ToString();

            Assert.AreEqual(e1, e2);
            Assert.AreEqual(e2, e3);

            var r1 = q1.ToArray();
            var r2 = q2.ToArray();
            var r3 = q3.ToArray();

            Assert.IsTrue(r1.SequenceEqual(r2));
            Assert.IsTrue(r2.SequenceEqual(r3));
        }
    }
}
