using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tonic.UI
{
    /// <summary>
    /// An auto grid panel with two columns
    /// </summary>
    public class AutoGridN : AutoGrid
    {
        /// <summary>
        /// Create a new auto grid
        /// </summary>
        public AutoGridN()
        {

            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            ChildMargin = new System.Windows.Thickness(3);
            OnPairCountChanged();
        }


        /// <summary>
        /// Number of auto column pairs
        /// </summary>
        public int N
        {
            get { return (int)GetValue(NProperty); }
            set { SetValue(NProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PairCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NProperty =
            DependencyProperty.Register("N", typeof(int), typeof(AutoGridN), new PropertyMetadata(1, (o, e) => ((AutoGridN)o).OnPairCountChanged()));



        private void OnPairCountChanged()
        {
            ColumnDefinitions.Clear();
            for (int i = 0; i < N; i++)
            {
                ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
                ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
            }
        }
    }
}
