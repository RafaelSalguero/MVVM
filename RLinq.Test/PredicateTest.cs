using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tonic;
namespace RLinq.Test
{
    [TestClass]
    public class PredicateTest
    {
        class Persona
        {
            public int? Columna { get; set; }
        }

        [TestMethod]
        public void TestPredicateEqual()
        {
            var P = Tonic.PredicateBuilder.PredicateEqual(typeof(Persona), nameof(Persona.Columna), 3);

            var Personas = new[]
            {
                new Persona { Columna=1 },
                new Persona { Columna=2 },
                new Persona { Columna=3 },
                new Persona { Columna=4 },
                new Persona { Columna=null },
            };

            Assert.AreEqual(Personas[2], Personas.Where(P).First());

            P = Tonic.PredicateBuilder.PredicateEqual(typeof(Persona), nameof(Persona.Columna), null);
            Assert.AreEqual(Personas[4], Personas.Where(P).First());
        }
    }
}
