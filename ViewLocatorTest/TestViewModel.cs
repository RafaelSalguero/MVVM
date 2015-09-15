using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM;

namespace ViewLocatorTest
{
    public class TestViewModel : ExposedViewModel<TestModel>
    {
        public TestViewModel()
        {
            Model = new TestModel();
        }
    }
}
