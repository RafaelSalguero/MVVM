using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tonic.UI
{
    /// <summary>
    /// Interaction logic for ButtonDock.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class ButtonDock : UserControl
    {
        public ButtonDock()
        {
            InitializeComponent();

            Children = Grid.Children;
            Buttons = ButtonStack.Children;

        }

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Children.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey ChildrenProperty =
            DependencyProperty.RegisterReadOnly("Children", typeof(UIElementCollection), typeof(ButtonDock), new PropertyMetadata(null));




        public UIElementCollection Buttons
        {
            get { return (UIElementCollection)GetValue(ButtonsProperty.DependencyProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Buttons.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey ButtonsProperty =
            DependencyProperty.RegisterReadOnly("Buttons", typeof(UIElementCollection), typeof(ButtonDock), new PropertyMetadata(null));





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
