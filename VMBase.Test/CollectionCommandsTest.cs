using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.MVVM;
using Tonic.MVVM.Extensions;

namespace VMBase.Test
{
    [TestClass]
    public class CollectionCommandsTest
    {
        public class Element : ExposedViewModel<object>
        {

        }

        public class ViewModelSI : BaseViewModel
        {
            public ViewModelSI()
            {
                AddExtension(CollectionCommandsExtension.Create(this, x => x.Items, x => x.SelectedItem, x => { }, x => true));
            }

            public ICollection<Element> Items { get; set; }
            public Element SelectedItem { get; set; }
        }

        public class ViewModelESI : BaseViewModel
        {
            public ViewModelESI()
            {
                AddExtension(CollectionCommandsExtension.Create(this, x => x.Items, x => { }, x => true));
            }

            public ICollection<Element> Items { get; set; }
        }

        [TestMethod]
        public void CollectionCommandTest()
        {
            dynamic V = new ViewModelSI();
            Assert.AreNotEqual(null, V.ItemsAddCommand);
            Assert.AreNotEqual(null, V.ItemsEditCommand);
            Assert.AreNotEqual(null, V.ItemsRemoveCommand);

            V = new ViewModelESI();
            Assert.AreNotEqual(null, V.ItemsAddCommand);
            Assert.AreNotEqual(null, V.ItemsEditCommand);
            Assert.AreNotEqual(null, V.ItemsRemoveCommand);

            Assert.AreEqual(null, V.ItemsSelectedItem);
            V.ItemsSelectedItem = new Element();

            Assert.AreNotEqual(null, V.ItemsSelectedItem);


        }
    }
}
