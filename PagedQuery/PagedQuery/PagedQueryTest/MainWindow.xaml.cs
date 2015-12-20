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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PagedQueryTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            using (var G = new Model.G5())
            {
                var r = G.MOV_VentasDhola.OrderBy(x => x.Fecha);
            }

            InitializeComponent();
            this.DataContext = new ViewModel.QueryTest();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         
        }
    }


}
