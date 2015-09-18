using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.MVVM;
using Tonic.MVVM.Extensions;
using System.Linq;
namespace VMBase.Test
{
    [TestClass]
    public class UnitTest1
    {
        public class CViewModel : BaseViewModel
        {
            public CViewModel()
            {
                AddExtension(new CommandsExtension(this));
            }

            public bool Agregado = false;

            public void Agregar()
            {
                Agregado = true;
            }
        }

        public class Model : IDataErrorInfo
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }

            string IDataErrorInfo.Error
            {
                get
                {
                    return "";
                }
            }

            string IDataErrorInfo.this[string columnName]
            {
                get
                {
                    if (columnName == nameof(Nombre))
                    {
                        if (Nombre == "Coco")
                            return "El nombre no puede ser 'coco'";
                    }
                    return null;
                }
            }
        }

        public class MViewModel : ExposedViewModel<Model>
        {

        }

        [TestMethod]
        public void ModelTest()
        {
            var VM = (dynamic)(new MViewModel());
            var err = (INotifyDataErrorInfo)VM;
            var prop = (INotifyPropertyChanged)VM;

            var PropChanged = new List<string>();
            var ErrChanged = new List<string>();

            err.ErrorsChanged += (a, b) => ErrChanged.Add(b.PropertyName);
            prop.PropertyChanged += (a, b) => PropChanged.Add(b.PropertyName);

            //Establece el modelo:
            VM.Model = new Model();

            //Todas las propiedades debieron de haber cambiado:
            Assert.IsTrue(PropChanged.SequenceEqual(new string[] { "Nombre", "Apellido", "Model" }));

            PropChanged.Clear();
            VM.Nombre = "Rafael";

            //El nombre debio de haber cambiado:
            Assert.AreEqual("Rafael", VM.Model.Nombre);
            Assert.AreEqual("Rafael", VM.Nombre);

            Assert.IsTrue(PropChanged.SequenceEqual(new string[] { "Nombre" }));

            PropChanged.Clear();

            //Este valor no esta permitido:
            VM.Nombre = "Coco";

            Assert.AreEqual("Coco", VM.Nombre);
            Assert.IsTrue(PropChanged.SequenceEqual(new string[] { "Nombre" }));
            Assert.IsTrue(ErrChanged.SequenceEqual(new string[] { "Nombre" }));

            Assert.AreEqual("El nombre no puede ser 'coco'", err.GetErrors("Nombre").Cast<string>().Single());

            PropChanged.Clear();
            ErrChanged.Clear();

            //Cambia el nombre a otro que si esta permitido:

            VM.Nombre = "Rafael";

            Assert.AreEqual("Rafael", VM.Nombre);
            Assert.IsTrue(PropChanged.SequenceEqual(new string[] { "Nombre" }));

            //Se notifico que la propiedad nombre cambio sus errores
            Assert.IsTrue(ErrChanged.SequenceEqual(new string[] { "Nombre" }));

            //Ya no tiene errores
            Assert.AreEqual(false, err.GetErrors("Nombre").Cast<string>().Any());
        }

        [TestMethod]
        public void CommandsTest()
        {
            //Crea un nuevo view model
            var VM = new CViewModel();
            //El metodo agregar debe de estar expuesto como un comando:
            var C = (ICommand)((dynamic)VM).AgregarCommand;

            //Ejecuta el comando:
            Assert.AreEqual(false, VM.Agregado);
            C.Execute(null);

            //El comando se ejecuto correctamente:
            Assert.AreEqual(true, VM.Agregado);
        }
    }
}
