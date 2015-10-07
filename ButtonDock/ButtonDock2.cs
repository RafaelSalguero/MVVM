using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Tonic.UI
{
    /// <summary>
    /// Contains content and buttons
    /// </summary>
    public class ButtonDock2 : ContentControl
    {
        static ButtonDock2 ()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonDock2), new FrameworkPropertyMetadata(typeof(ButtonDock2)));
        }

        /// <summary>
        /// Create a new ButtonDock
        /// </summary>
        public ButtonDock2()
        {
            Buttons = new List<FrameworkElement>();
        }

        /// <summary>
        /// A collection of buttons
        /// </summary>
        public IList Buttons
        {
            get { return (UIElementCollection)GetValue(ButtonsProperty.DependencyProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Buttons.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey ButtonsProperty =
            DependencyProperty.RegisterReadOnly("Buttons", typeof(IList), typeof(ButtonDock2), new PropertyMetadata(null));

        /// <summary>
        /// Buttons alignment
        /// </summary>
        public HorizontalAlignment ButtonAlignment
        {
            get { return (HorizontalAlignment)GetValue(ButtonAlignmentProperty); }
            set { SetValue(ButtonAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonAlignmentProperty =
            DependencyProperty.Register("ButtonAlignment", typeof(HorizontalAlignment), typeof(ButtonDock2), new PropertyMetadata(HorizontalAlignment.Right));



    }
}
