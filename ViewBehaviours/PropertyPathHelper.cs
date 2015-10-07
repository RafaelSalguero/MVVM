using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Tonic.UI
{
    static class PropertyPathHelper
    {
       
        public static object GetValue(object obj, string propertyPath)
        {
            Binding binding = new Binding(propertyPath);
            binding.Mode = BindingMode.OneTime;
            binding.Source = obj;
            BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
            return _dummy.GetValue(Dummy.ValueProperty);
        }

        public static object GetTargetType(object obj, string propertyPath)
        {
            Binding binding = new Binding(propertyPath);
            binding.Mode = BindingMode.OneTime;
            binding.Source = obj;
            BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
            return _dummy.GetValue(Dummy.ValueProperty);
        }

        public static void SetValue(object obj, string propertyPath, object Value)
        {
            Binding binding = new Binding(propertyPath);
            binding.Mode = BindingMode.TwoWay;
            binding.Source = obj;
            BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
            _dummy.SetValue(Dummy.ValueProperty, Value);
        }

        private static readonly Dummy _dummy = new Dummy();

        private class Dummy : DependencyObject
        {
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(Dummy), new UIPropertyMetadata(null));
        }
    }
}
