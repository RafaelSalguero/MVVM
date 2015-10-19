using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM;

namespace ViewLocatorTest
{
    public class Locator : ViewModelLocator
    {
        private static INameLocator GetLocator()
        {
            var P = new PairLocator(null);
            P.Add(typeof(TestViewModel));

            return P;
        }

        public Locator(string Name) : base(GetLocator(), Name) { }
    }
}
