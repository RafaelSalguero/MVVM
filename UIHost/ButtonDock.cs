using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ButtonDock : ContentControl
    {
        static ButtonDock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonDock), new FrameworkPropertyMetadata(typeof(ButtonDock)));
        }

        /// <summary>
        /// Create a new ButtonDock
        /// </summary>
        public ButtonDock()
        {
            Buttons = new ObservableCollection<FrameworkElement>();
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
            DependencyProperty.RegisterReadOnly("Buttons", typeof(IList), typeof(ButtonDock), new PropertyMetadata(null));

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
            DependencyProperty.Register("ButtonAlignment", typeof(HorizontalAlignment), typeof(ButtonDock), new PropertyMetadata(HorizontalAlignment.Right));



    }
}
