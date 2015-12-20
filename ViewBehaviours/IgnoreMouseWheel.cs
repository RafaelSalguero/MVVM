using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Tonic.UI
{
    public static class IgnoreMouseWheelBehavior
    {
        public static bool GetEnabled(UIElement gridItem)
        {
            return (bool)gridItem.GetValue(EnabledProperty);
        }

        public static void SetEnabled(UIElement gridItem, bool value)
        {
            gridItem.SetValue(EnabledProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool),
            typeof(IgnoreMouseWheelBehavior), new UIPropertyMetadata(false, OnEnabledChanged));

        static void OnEnabledChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var item = depObj as UIElement;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                item.PreviewMouseWheel += OnPreviewMouseWheel;
            else
                item.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            { RoutedEvent = UIElement.MouseWheelEvent };

            var gv = sender as UIElement;
            if (gv != null) gv.RaiseEvent(e2);
        }
    }
}
