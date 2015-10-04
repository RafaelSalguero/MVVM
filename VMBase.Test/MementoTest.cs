using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VMBase.Test
{
    [TestClass]
    public class MementoTest
    {
        public class Persona
        {
            public int Edad { get; set; }
            public string Nombre { get; set; }

            public virtual string Hola
            {
                get
                {
                    return "Hola";
                }
            }

            public bool LazyLoaded = false;
            public bool LazyAssigned = false;

            private string lazy;
            public virtual string Lazy
            {
                get
                {
                    LazyLoaded = true;
                    return lazy;
                }
                set
                {
                    LazyAssigned = true;
                    lazy = value;
                }
            }
        }

        [TestMethod]
        public void Memento()
        {
            var P = new Persona { Edad = 21, Nombre = "Rafael" };

            //Se asegura de que las propiedades lazy no han sido cargadas:
            Assert.AreEqual(false, P.LazyAssigned);
            Assert.AreEqual(false, P.LazyLoaded);

            P.Lazy = "Lazy 1";

            //Se asegura de que las propiedades lazy fue asignada pero no leida
            Assert.AreEqual(true, P.LazyAssigned);
            Assert.AreEqual(false, P.LazyLoaded);

            //Crea un memento:
            IEnumerable<string> CopyProperties;
            var P2 = Tonic.MVVM.MementoFactory.Lazy(P, out CopyProperties);

            Assert.IsTrue(CopyProperties.OrderBy(x => x).SequenceEqual(new[] { "Edad", "Nombre" }));



            //Resetea el LazyAssigned:
            P.LazyAssigned = false;

            //Se asegura de que las propiedades lazy no han sido cargadas en ninguna de las dos clases:
            //Esto significa que la creación del memento nunca accedio a las propiedades virtuales
            Assert.AreEqual(false, P.LazyAssigned);
            Assert.AreEqual(false, P.LazyLoaded);

            Assert.AreEqual(false, P2.LazyAssigned);
            Assert.AreEqual(false, P2.LazyLoaded);

            //Se asegura que las dos propiedades virtuales sean iguales, esto debe disparara el LazyLoaded de la primera clase solamente,
            //la segunda clase no lo dispara porque el memento en realidad llama al lazy de la primera clase para obtener su valor
            var v1 = P.Lazy;
            Assert.AreEqual(true, P.LazyLoaded);
            Assert.AreEqual(false, P2.LazyLoaded);

            P.LazyLoaded = false;
            var v2 = P2.Lazy;
            Assert.AreEqual(true, P.LazyLoaded);
            Assert.AreEqual(false, P2.LazyLoaded);

            Assert.AreEqual(v1, v2);

            P.LazyLoaded = false;
            //La segunda ves que se obtiene el valor de P2 ya no se dispara la carga, porque esta ya esta almacenada en el cache del memento:
            Assert.AreEqual(v1, P2.Lazy);

            Assert.AreEqual(false, P.LazyLoaded);
            Assert.AreEqual(false, P2.LazyLoaded);

            //Si se cambia el valor de P1 despues de haber asignado P2, el valor de P2 debe de seguir siendo el original
            P.Nombre = "Alesita";
            P.Edad = 20;
            P.Lazy = "Lazy 2";

            Assert.AreEqual(true, P.LazyAssigned);
            Assert.AreEqual("Rafael", P2.Nombre);
            Assert.AreEqual(21, P2.Edad);
            Assert.AreEqual("Lazy 1", P2.Lazy);

            P.LazyAssigned = false;

            Assert.IsTrue(CopyProperties.OrderBy(x => x).SequenceEqual(new[] { "Edad", "Nombre" }));

            //Si se asigna la propiedad virtual de P2, el valor de P debe de conservarse igual, y
            //el lazy asigned debe de ser falso porque el memento debió de haber interceptado
            //el set de la propiedad
            P2.Lazy = "Nuevo 1";

            Assert.IsTrue(CopyProperties.OrderBy(x => x).SequenceEqual(new[] { "Edad", "Lazy", "Nombre" }));

            Assert.AreEqual(false, P2.LazyAssigned);
            Assert.AreEqual(false, P.LazyAssigned);

            Assert.AreEqual("Lazy 2", P.Lazy);
            Assert.AreEqual("Nuevo 1", P2.Lazy);

            Assert.AreEqual(false, P2.LazyLoaded);
            Assert.AreEqual(true, P.LazyLoaded);
        }
    }
}
