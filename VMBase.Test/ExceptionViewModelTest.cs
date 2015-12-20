using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.MVVM.Dialogs;
using Tonic.MVVM.Interfaces;

namespace VMBase.Test
{
    [TestClass]
    public class ExceptionViewModelTest
    {
        [TestMethod]
        public void ExceptionStructure()
        {
            ExceptionViewModel VM;
            try
            {
                Task.WaitAll(
                    Task.Run(() => { throw new ApplicationException("Excepcion de prueba", new ArgumentException("Argumento incorrecto", new InvalidOperationException("Division entre 0"))); }),
                    Task.Run(() => { throw new ApplicationException("Excepcion de prueba 2", new ArgumentException("Argumento incorrecto 3")); }),
                    Task.Run(() => { throw new ApplicationException("Excepcion de prueba 3", new ArgumentException("Argumento incorrecto 3")); }),
                    Task.Run(() => { throw new ApplicationException("Excepcion de prueba", new ArgumentException("Argumento incorrecto", new InvalidOperationException("Division entre 0"))); })
                );

            }
            catch (Exception ex)
            {
                VM = new ExceptionViewModel(ex, new ClipboardMock(), new AssemblyMock());
            }

            
        }
    }
}
