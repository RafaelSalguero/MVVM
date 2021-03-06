﻿using System;
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
    public class ViewModelLocator : System.Windows.Markup.MarkupExtension
    {
        private INameLocator locator;
        readonly string name;
        public ViewModelLocator(INameLocator locator, string Name)
        {
            this.locator = locator;
            this.name = Name;
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var Instance = locator.Get(name);
            return Instance;
        }
    }
}
