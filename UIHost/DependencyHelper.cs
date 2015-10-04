using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tonic.UI
{
    static class DependencyHelper
    {
        /// <summary>
        ///     Sets the value of the <paramref name="property" /> only if it hasn't been explicitely set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool SetIfDefault<T>(this DependencyObject o, DependencyProperty property, T value)
        {
            if (!property.PropertyType.IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("Dependency property incompatible type");
            }

            if (DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource == BaseValueSource.Default)
            {
                o.SetValue(property, value);

                return true;
            }

            return false;
        }
    }
}
