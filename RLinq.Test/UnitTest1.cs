using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;

namespace RLinq.Test
{
    [TestClass]
    public class UnitTest1
    {
        public class Artist
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int APlusB
            {
                get
                {
                    return this.Invoke(e => e.Select(() =>
                    A + B
                    ));
                }
            }
            public string NombreCompleto
            {
                get
                {
                    return this.Invoke(e => e.Select(() => Nombre + " " + Apellido));
                }
            }

            public ICollection<Track> Tracks { get; set; } = new HashSet<Track>();
        }

        public class Track
        {
            public string Title { get; set; }
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
        public void RLinqThisInvoke()
        {
            var itemarray = new[]
            {
                new Artist { Nombre = "Alejandra", Apellido = "Llanez" },
                new Artist { Nombre = "Rafael", Apellido = "Salguero" }
            };

            var items = (itemarray).AsQueryable().Decompile();
            var Q = items.Where(x => x.NombreCompleto == "Rafael Salguero");
        }

        [TestMethod]
        public void RLinqExpand()
        {
            Expression<Func<Artist, bool>> Predicate;

            Predicate = ar => ar.Apellido == "Salguero";

            Expression<Func<Artist, bool>> Pred2;
            Pred2 = ar => Predicate.Invoke(ar);

            Expression<Func<Artist, bool>> PredExpand = Pred2.Decompile();

            Assert.AreNotEqual(Pred2.ToString(), Predicate.ToString());

            Assert.AreEqual(PredExpand.ToString(), Pred2.Decompile().ToString());
            Assert.AreEqual(PredExpand.ToString(), Predicate.ToString());
        }

        [TestMethod]
        public void RLinqQueryExpand()
        {
            Expression<Func<Track, bool>> Predicate;
            Predicate = tr => tr.Title == "Back In Black";

            var itemarray = new[]
           {
                new Artist { Nombre = "Alejandra", Apellido = "Llanez" },
                new Artist { Nombre = "Rafael", Apellido = "Salguero" }
            };
            itemarray[0].Tracks.Add(new Track { Title = "Back In Black" });

            var items = (itemarray).AsQueryable();


            var Query = items.Where(ar => ar.Tracks.Any(Predicate.Compile()));
            var QueryDec = items.Decompile().Where(ar => ar.Tracks.Any(Predicate.Compile()));

            var QueryExp = items.Where(ar => ar.Tracks.Any(tr => tr.Title == "Back In Black"));

            Assert.AreEqual(QueryDec.ToString(), QueryExp.ToString());
            Assert.AreNotEqual(Query.ToString(), QueryExp.ToString());

            Assert.AreEqual(Query.First(), QueryDec.First());
            Assert.AreEqual(QueryDec.First(), QueryExp.First());
        }

        [TestMethod]
        public void RLinqFormat()
        {
            string Sql = "select \"Extent1\".\"nombre\" from \"public\".\"cliente\" AS \"Extent1\" ";
            var R = Tonic.RLinq.FormatSql(Sql);

        }

        [TestMethod]
        public void RLinqDecompileDatetimeTimespan()
        {
            var items = new[]
            {
                new { Date = new DateTime (2015,01,26), Time = new TimeSpan (5) },
                new { Date = new DateTime (2015,02,14), Time = new TimeSpan (8) },
            }.AsQueryable();

            var dec = items.Decompile();

            var Max = items.Select(x => x.Date + x.Time);
            var MaxDec = dec.Select(x => x.Date + x.Time);
            var MacN = items.Select(x => DbFunctions.AddMilliseconds(x.Date, DbFunctions.DiffMilliseconds(TimeSpan.Zero, x.Time)).Value);

            Assert.AreEqual(MacN.ToString(), MaxDec.ToString());

            Assert.IsTrue(Max.SequenceEqual(MaxDec));
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

        [TestMethod]
        public void RLinqNeastedDecompile()
        {
            var array = new[]
              {
                new Artist { Nombre = "Alejandra", Apellido = "Llanez", A= 2, B = 3  },
                new Artist { Nombre = "Rafael", Apellido = "Salguero" , A = 4, B = 5 }
            };


            var suma1 = array[0].APlusB;
            var suma2 = array[1].APlusB;

            var items = (array).AsQueryable();

            var q1 = items.Decompile()
                .Where(x => x.APlusB > 2)
                .Sum(x => x.APlusB);

        }


    }
}
