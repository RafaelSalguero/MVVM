using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    class Locator : ViewModelLocator
    {
        public static IServiceProvider Designer()
        {
            var Kernel = new Ninject.StandardKernel();
            Kernel.Bind<Tonic.MVVM.Dialogs.ProgressViewModel>().ToMethod(x => new Dialogs.ProgressViewModel { Title = "Titulo de prueba", Message = "Mensaje de prueba" });

            return Kernel;
        }

        static INameLocator GetLocator()
        {
            var P = new PairLocator(Designer());
            P.Add(typeof(Tonic.MVVM.Dialogs.ProgressViewModel), "Progress");
            return P;
        }

        public Locator(string Name) : base(GetLocator(), Name) { }
    }
}
