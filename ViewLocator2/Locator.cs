using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Interfaces;

namespace Tonic.MVVM
{
    class Locator : ViewModelLocator
    {
        public static IServiceProvider Designer()
        {
            var Kernel = new Ninject.StandardKernel();
            Kernel.Bind<Tonic.MVVM.Dialogs.ProgressViewModel>().ToMethod(x => new Dialogs.ProgressViewModel { Title = "Titulo de prueba", Message = "Mensaje de prueba", Value = 0.5 });
            Kernel.Bind<Tonic.MVVM.Dialogs.ExceptionViewModel>().ToMethod(x =>
          {
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
                  return new Dialogs.ExceptionViewModel(ex, new ClipboardMock(), new AssemblyMock());
              }
              throw new ApplicationException();
          });

            return Kernel;
        }

        static INameLocator GetLocator()
        {
            var P = new PairLocator(Designer());
            P.Add(typeof(Tonic.MVVM.Dialogs.ProgressViewModel));
            P.Add(typeof(Tonic.MVVM.Dialogs.ExceptionViewModel));
            return P;
        }

        public Locator(string Name) : base(GetLocator(), Name) { }
    }
}
