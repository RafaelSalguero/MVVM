using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM;

namespace ExampleView2
{

    public class Locator : ViewModelLocator
    {
        public static IServiceProvider Designer()
        {
            var Kernel = new Ninject.StandardKernel();
            Kernel.Bind<IView>().To<ViewMock>();

            Func<ExampleData.ExampleContext> Context = () => new ExampleData.ExampleContext(Effort.DbConnectionFactory.CreateTransient());
            Kernel.Bind<Func<ExampleData.ExampleContext>>().ToMethod(x => Context);

            return Kernel;
        }

        private static INameLocator GetLocator()
        {
            var P = new PairLocator(Designer());
            foreach (var N in ConventionLocator.Locate(typeof(MainView), typeof(ExampleViewModel.MainViewModel)))
                P.Add(N);
            return P;
        }
        public Locator() : base(GetLocator())
        {
        }
    }
}
