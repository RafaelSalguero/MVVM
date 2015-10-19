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
    /// <summary>
    /// Converts true to Visible, false to Hidden, and null to Collapsed
    /// </summary>
    public class BoolNullVisibility : IValueConverter
    {
        /// <summary>
        /// Visibility value for a boolean true. Default is Visible
        /// </summary>
        public Visibility True { get; set; } = Visibility.Visible;

        /// <summary>
        /// Visibility value for a boolean false. Default is Hidden
        /// </summary>
        public Visibility False { get; set; } = Visibility.Hidden;

        /// <summary>
        /// Visibility value for a null. Default is collapsed
        /// </summary>
        public Visibility Null { get; set; } = Visibility.Collapsed;


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Null;
            else if (Equals(value, false))
                return False;
            else if (Equals(value, true))
                return True;
            else
                return DependencyProperty.UnsetValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, True))
                return true;
            else if (Equals(value, False))
                return false;
            else if (Equals(value, Null))
                return null;
            else
                return DependencyProperty.UnsetValue;
        }
    }
}
