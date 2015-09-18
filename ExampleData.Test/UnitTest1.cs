using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.EF;
using Tonic.MVVM;
using Tonic;
namespace ExampleData.Test
{
    [TestClass]
    public class UnitTest1
    {
        public class ArtistViewModel : EntityViewModel<Artist, ExampleContext>
        {
            public ArtistViewModel(Func<ExampleContext> Context) : base(Context) { }
            public ArtistViewModel(Func<ExampleContext> Context, Artist Model) : base(Context) { this.Model = Model; }
            public ICollection<AlbumViewModel> Albums
            {
                get
                {
                    return GetCollection(nameof(Albums), x => new AlbumViewModel(x));
                }
            }

            public Guid ArtistId
            {
                get
                {
                    return Model.ArtistId;
                }
            }
        }
        public class AlbumViewModel : EntityViewModel<Album, ExampleContext>
        {
            public AlbumViewModel(Func<ExampleContext> Context) : base(Context) { }

        }

        [TestMethod]
        public void PagedQueryIntegrationTest()
        {
            var Conn = Effort.DbConnectionFactory.CreateTransient();
            Func<ExampleContext> context = () => new ExampleContext(Conn);

            //Agrega algunos artistas:
            Artist Rafa;
            using (var C = context())
            {
                Rafa = new Artist { Name = "Alejandra" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Alejandra" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);
                C.SaveChanges();
            };


            //Este query debe de funcionar porque se ejecuta en dos partes
            using (var C = context())
            {
                bool hit = false;
                var hitCall = C.Database.Log += e => hit = true;

                //El where se debe de ejecutar en el lado del servidor, y el ultimo select en el lado del cliente
                var it2 = C.Artist
                    .Where(x => x.Name == "Rafael")
                    .OrderBy(x => x.Name)
                    .ToClient()
                    .Select(x => new ArtistViewModel(context, x));

                var r2 = it2.ToArray();
                var PagedQuery = Tonic.Patterns.PagedQuery.QueryFactory.Create(it2, 3, 1);

                //La primera lectura debe de ocasionar una operacion de la base de datos
                var ar1 = ((IList<ArtistViewModel>)PagedQuery)[0];
                Assert.AreEqual(true, hit);

                hit = false;

                //La segunda lectura no ocasiona una operacion, pues la pagina de tamaño 3 ya lo tiene
                var ar2 = ((IList<ArtistViewModel>)PagedQuery)[1];
                Assert.AreEqual(false, hit);

                //La tercera lectura tambien se encuentra ya en memoria
                var ar3 = ((IList<ArtistViewModel>)PagedQuery)[2];
                Assert.AreEqual(false, hit);

                //La cuarta lectura tambien se encuentra fuera de la pagina, asi que si ocasiona una operacion de la base de datos
                var ar4 = ((IList<ArtistViewModel>)PagedQuery)[3];
                Assert.AreEqual(true, hit);


                //Verifica que los elementos sean los correctos:
                Assert.AreEqual(ar1.ArtistId, r2[0].ArtistId);
                Assert.AreEqual(ar2.ArtistId, r2[1].ArtistId);
                Assert.AreEqual(ar3.ArtistId, r2[2].ArtistId);
                Assert.AreEqual(ar4.ArtistId, r2[3].ArtistId);
            }
        }

        [TestMethod]
        public void ClientServerQueryReorder()
        {
            var Conn = Effort.DbConnectionFactory.CreateTransient();
            Func<ExampleContext> context = () => new ExampleContext(Conn);

            //Agrega algunos artistas:
            Artist Rafa;
            using (var C = context())
            {
                Rafa = new Artist { Name = "Alejandra" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Alejandra" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);

                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);
                C.SaveChanges();
            };


            //Este query debe de fallar porque se intenta instanciar una clase en Linq to Entities
            {
                bool fail = false;
                try
                {
                    using (var C = context())
                    {
                        var it2 = C.Artist.Where(x => x.Name == "Rafael").Select(x => new ArtistViewModel(context, x));

                        var Result = it2.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    fail = true;
                }
                Assert.AreEqual(true, fail);
            }

            //Este query debe de funcionar porque se ejecuta en dos partes
            using (var C = context())
            {
                //El where se debe de ejecutar en el lado del servidor, y el ultimo select en el lado del cliente
                var it2 = C.Artist
                    .Where(x => x.Name == "Rafael")
                    .ToClient()
                    .Select(x => new ArtistViewModel(context, x));

                //Query utilizando ToClient y queries en el lado del cliente y del servidor
                var Result = it2.ToArray().Cast<dynamic>().Select(x => x.Name);

                //Query completamente en memoria:
                var R2 = C.Artist.ToList()
                    .Where(x => x.Name == "Rafael").Select(x => new ArtistViewModel(context, x)).ToArray().Cast<dynamic>().Select(x => x.Name);

                Assert.AreEqual(true, Result.SequenceEqual(R2));
            }

            //El take se debe de ejecutar en el lado del servidor, aunque este despues del ToClient
            using (var C = context())
            {
                //El where y el take se debe de ejecutar en el lado del servidor, y el ultimo select en el lado del cliente.
                var it2 = C.Artist
                    .Where(x => x.Name == "Rafael")
                    .ToClient()
                    .Select(x => new ArtistViewModel(context, x))
                    .Take(1);

                var it3 = C.Artist.Where(x => x.Name == "Rafael").Take(1)
                    .ToClient()
                    .Select(x => new ArtistViewModel(context, x));

                //Las tres expresiones tienen la misma representacion de cadena porque el ToString del ServerSideQuery devuelve siempre lo mismo
                //sin importar su contenido

                //Note que el Take de it2 no afecta porque este es transladado al server side
                var ex1 = it2.Expression.ToString();
                var ex2 = C.Artist.ToClient().Select(x => new ArtistViewModel(context, x)).Expression.ToString();
                var ex3 = it3.Expression.ToString();

                Assert.AreEqual(ex1, ex2);
                Assert.AreEqual(ex2, ex3);

                var Result1 = it2.ToArray().Cast<dynamic>().Select(x => x.Name);
                var Result2 = it3.ToArray().Cast<dynamic>().Select(x => x.Name);

                Assert.AreEqual(true, Result1.SequenceEqual(Result2));
            }

            //Se ejecutara un Count, este se debe de realizar en el lado del servidor
            using (var C = context())
            {
                //El where y el count se debe de ejecutar en el lado del servidor, el último select nunca es ejecutado
                var it2 = C.Artist
                    .Where(x => x.Name == "Rafael")
                    .ToClient()
                    .Select(x => new ArtistViewModel(context, x))
                    .Count();

                Assert.AreEqual(C.Artist.Where(x => x.Name == "Rafael").Count(), it2);
            }
        }


        [TestMethod]
        public void EntityViewModelTest()
        {
            var Conn = Effort.DbConnectionFactory.CreateTransient();
            Func<ExampleContext> context = () => new ExampleContext(Conn);

            //Agrega un artista con un album
            Artist Rafa;
            Album Fruits;
            using (var C = context())
            {
                Rafa = new Artist { Name = "Rafael" };
                Fruits = new Album { Title = "Fruits" };
                Rafa.Albums.Add(Fruits);
                C.Artist.Add(Rafa);
                C.SaveChanges();
            };

            //Obtiene un entity view model a partir del artista rafael
            var RafaVM = new ArtistViewModel(context);

            ICollection<AlbumViewModel> albums;
            using (var C = context())
            {
                RafaVM.Model = C.Artist.First();
            }


            //Toda la informacion del artista ya debe de estar disponible afuera del contexto, incluyendo propiedades de navegacion:
            //Obtiene el nombre del artista:
            var name = (string)((dynamic)RafaVM).Name;
            Assert.AreEqual("Rafael", name);

            albums = RafaVM.Albums;
            Assert.AreEqual("Fruits", ((dynamic)albums.First()).Title);

            //Borra el primer album
            albums.Remove(albums.First());

            //Modifica el nombre del artista:
            ((dynamic)RafaVM).Name = "Rafaelito";


            //Verifica que no se ha realizado ningun cambio a la base de datos:
            using (var C = context())
            {
                Assert.AreEqual("Rafael", C.Artist.First().Name);
                Assert.AreEqual(1, C.Album.Count());
            }

            //Guarda los cambios, equivalente a dar clic en OK
            using (var C = context())
            {
                RafaVM.Persist(C);
                C.SaveChanges();
            }

            //Verifica que se realizaron los cambios
            using (var C = context())
            {
                Assert.AreEqual("Rafaelito", C.Artist.First().Name);
                Assert.AreEqual(0, C.Album.Count());
            }
        }
        [TestMethod]
        public void TestDeleteTest()
        {
            var Conn = Effort.DbConnectionFactory.CreateTransient();

            //Agrega un artista con un album
            Artist Rafa;
            Album Fruits;
            using (var C = new ExampleContext(Conn))
            {
                Rafa = new Artist { Name = "Rafael" };
                Fruits = new Album { Title = "Fruits" };
                Rafa.Albums.Add(Fruits);
                C.Artist.Add(Rafa);
                C.SaveChanges();
            }

            {
                bool fail = false;
                try
                {
                    //Trata borra el artista en una transaccion, debe de fallar pues el artista tiene hijos relacionados
                    using (var C = new ExampleContext(Conn))
                    {
                        using (var T = C.Database.BeginTransaction())
                        {
                            C.Artist.Remove(C.Artist.First());
                            C.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    fail = true;
                }
                Assert.AreEqual(true, fail);
            }

            //Prueba el can delete, debe de ser falso
            Assert.AreEqual(false, DbContextExtensions.CanDelete(() => new ExampleContext(Conn), Rafa));

            //Borra el album
            using (var C = new ExampleContext(Conn))
            {
                C.Album.Remove(C.Album.First());
                C.SaveChanges();
            }

            //Can delete debe de ser true porque el artista ya no tiene albumes
            Assert.AreEqual(true, DbContextExtensions.CanDelete(() => new ExampleContext(Conn), Rafa));

            //Comprueba que el can delete no modifico la base de datos:
            using (var C = new ExampleContext(Conn))
            {
                //Falla si no hay ningun artista:
                var A = C.Artist.First();
            }
        }

        [TestMethod]
        public void TransactionTest()
        {
            var Conn = Effort.DbConnectionFactory.CreateTransient();


            //Agrega un artista 
            Artist Rafa;
            using (var C = new ExampleContext(Conn))
            {
                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);
                C.SaveChanges();
            }

            //Trata borra el artista en una transaccion, no se debe de borrar pues la transaccion no hace commit
            using (var C = new ExampleContext(Conn))
            {
                using (var T = C.Database.BeginTransaction())
                {
                    C.Artist.Remove(C.Artist.First());
                    C.SaveChanges();
                }
            }

            using (var C = new ExampleContext(Conn))
            {
                //Falla si el artista si se borro:
                var A = C.Artist.First();
            }

            //Trata borra el artista en una transaccion, se debe de borrar pues la transaccion hace commit
            using (var C = new ExampleContext(Conn))
            {
                using (var T = C.Database.BeginTransaction())
                {
                    C.Artist.Remove(C.Artist.First());
                    C.SaveChanges();
                    T.Commit();
                }
            }

            using (var C = new ExampleContext(Conn))
            {
                Assert.AreEqual(false, C.Artist.Any());
            }
        }

        [TestMethod]
        public void AttachDetachTest()
        {
            var Conn = Effort.DbConnectionFactory.CreateTransient();

            //Agrega un artista como una operacion unitaria::
            Artist Rafa;
            using (var C = new ExampleContext(Conn))
            {
                Rafa = new Artist { Name = "Rafael" };
                C.Artist.Add(Rafa);
                C.SaveChanges();
            }

            //Ahora rafa es una entidad desconectada, este metodo debe de fallar porque Rafa es parte de un contexto diferente:
            bool fail = false;
            try
            {
                using (var C = new ExampleContext(Conn))
                {
                    //Crea un nuevo album
                    var Album = new Album { Title = "Fruits" };
                    Album.Artist = Rafa;
                    C.Album.Add(Album);
                    C.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                fail = true;
            }
            if (!fail) System.Diagnostics.Debug.Assert(false);

            //Vuelve a obtener los albumes, deben de ser 0 porque la operacion anterior fallo:
            using (var C = new ExampleContext(Conn))
            {
                Assert.AreEqual(0, C.Album.Count());
            }

            //Trata de borrar el artista que no esta atado al contexto:
            try
            {
                fail = false;
                using (var C = new ExampleContext(Conn))
                {
                    C.Artist.Remove(Rafa);
                    C.SaveChanges();
                }
            }
            catch (Exception)
            {
                fail = true;
            }
            if (!fail) System.Diagnostics.Debug.Assert(false);

            //Obtiene la entidad atada al contexto, y la borra
            using (var C = new ExampleContext(Conn))
            {
                var Rafa2 = C.Artist.GetEntity(Rafa);
                C.Artist.Remove(Rafa2);
                C.SaveChanges();
            }

            //Deben haber cero artistas ahora:
            using (var C = new ExampleContext(Conn))
            {
                Assert.AreEqual(0, C.Artist.Count());
            }
        }
    }

}