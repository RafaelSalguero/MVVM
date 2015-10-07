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

        class GetTypeConverter : IValueConverter
        {
            public Type lastType;
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                lastType = targetType;
                return lastType;
            }
        }


        public static Type  GetSourcePropertyType(object obj, string propertyPath)
        {
            Binding binding = new Binding(propertyPath);
            binding.Mode = BindingMode.TwoWay;
            binding.Source = obj;
            var Conv = new GetTypeConverter();
            binding.Converter = Conv;
            BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
            _dummy.SetValue(Dummy.ValueProperty, 1);
            return Conv.lastType;
        }

        private static readonly Dummy _dummy = new Dummy();

        private class Dummy : DependencyObject
        {
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(Dummy), new UIPropertyMetadata(null));
        }
    }
}
