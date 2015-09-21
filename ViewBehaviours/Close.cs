using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tonic.UI
{
    /// <summary>
    /// Close the window when the trigger property is set to true
    /// </summary>
    public static class Close
    {
        public static readonly DependencyProperty TriggerProperty =
            DependencyProperty.RegisterAttached(
                "Trigger",
                typeof(bool?),
                typeof(Close),
                new PropertyMetadata(TriggerChanged));

        private static void TriggerChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null && object.Equals(e.NewValue, true))
            {
                window.Close();
            }
        }
        public static void SetTrigger(Window target, bool? value)
        {
            target.SetValue(TriggerProperty, value);
        }
    }
}
