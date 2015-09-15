using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Provides view model instances by property name, given an INameLocator that locates view model instances. This class provides view models for views on design time
    /// </summary>
    public class ViewModelLocator : DynamicObject
    {
        private INameLocator locator;
        public ViewModelLocator(INameLocator locator)
        {
            this.locator = locator;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = locator.Get(binder.Name);
            return result != null;

        }
    }
}
