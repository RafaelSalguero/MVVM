using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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

        public static readonly DependencyProperty OnClickProperty =
            DependencyProperty.RegisterAttached(
                "OnClick",
                typeof(bool),
                typeof(Close),
                new PropertyMetadata(OnClickChanged));

        private static void OnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var aux = (ButtonBase)d;
            if (GetOnClick(aux))
            {
                aux.Click += CloseButtonHandler;

                //Esc key unicode:
                string Key = "\u001b";
                AccessKeyManager.Register(Key, aux);
                AccessKeyManager.AddAccessKeyPressedHandler(aux, EscapeKey);
            }
        }

        private static void EscapeKey(object sender, AccessKeyPressedEventArgs e)
        {
            var aux = (DependencyObject)sender;
            var ParentWindow = Tonic.UI.ParentTraverse.TrySearchDependencyObject<Window>(aux);
            if (ParentWindow != null)
                ParentWindow.Close();
        }



        private static void CloseButtonHandler(object sender, RoutedEventArgs e)
        {
            var aux = (ButtonBase)sender;
            var ParentWindow = Tonic.UI.ParentTraverse.TrySearchDependencyObject<Window>(aux);
            if (ParentWindow != null)
                ParentWindow.Close();
        }

        public static void SetOnClick(ButtonBase target, bool value)
        {
            target.SetValue(OnClickProperty, value);
        }
        public static bool GetOnClick(ButtonBase target)
        {
            return (bool)target.GetValue(OnClickProperty);
        }
    }
}
