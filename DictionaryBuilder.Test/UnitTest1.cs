using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.DictionaryBuilder;

namespace DictionaryBuilder.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DictionaryBuilderTest()
        {
            var Props = new[]
            {
                Tuple.Create (typeof(string), "Nombre", (object) "Rafael"),
                Tuple.Create (typeof(int ), "Edad", (object) 21)
            };

            var Properties = new TupleProperties(Props);
            var Instance = DictionaryBuilderFactory.Create(Properties);

            var InstanceType = Instance.GetType();
            var NombreProperty = InstanceType.GetProperty("Nombre");

            Assert.AreEqual(true, NombreProperty.CanRead);
            Assert.AreEqual(true, NombreProperty.CanWrite );

            Assert.AreEqual(NombreProperty.PropertyType, typeof(string));
            Assert.AreEqual(NombreProperty.GetValue(Instance), "Rafael");

            var EdadProperty = InstanceType.GetProperty("Edad");


            Assert.AreEqual(true, EdadProperty.CanRead);
            Assert.AreEqual(true, EdadProperty.CanWrite);


            Assert.AreEqual(EdadProperty.PropertyType, typeof(int));
            Assert.AreEqual(EdadProperty.GetValue(Instance), 21);

            NombreProperty.SetValue(Instance, "Alejandra");
            Assert.AreEqual("Alejandra", NombreProperty.GetValue(Instance));

            EdadProperty .SetValue(Instance, 20);
            Assert.AreEqual( 20 , EdadProperty.GetValue(Instance));

        }
    }
}

