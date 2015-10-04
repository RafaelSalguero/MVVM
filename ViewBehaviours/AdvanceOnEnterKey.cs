using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tonic.UI
{
    /// <summary>
    /// Contains the advance on enter key attached behaviour
    /// </summary>
    public static class AdvanceOnEnterKey
    {
        /// <summary>
        /// Enabled property getter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        /// <summary>
        /// Enabled property setter
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        /// <summary>
        /// Use this attached property setted to true to advance the focus when the user press the enter ket
        /// </summary>
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(AdvanceOnEnterKey),
            new UIPropertyMetadata(OnEnabledPropertyChanged));

        static void OnEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element == null) return;

            if ((bool)e.NewValue) element.PreviewKeyDown += Keydown;
            else element.PreviewKeyDown -= Keydown;
        }

        static void Keydown(object sender, KeyEventArgs e)
        {
            if (!e.Key.Equals(Key.Enter)) return;

            var element = e.OriginalSource as UIElement;

            if (element is TextBox)
            {
                var aux = (TextBox)element;
                if (aux.AcceptsReturn) return;
            }

            if (element != null)
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
    }
}
