using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.UI
{
    /// <summary>
    /// An auto grid panel with two columns
    /// </summary>
    public class AutoGrid2 : AutoGrid
    {
        /// <summary>
        /// Create a new auto grid
        /// </summary>
        public AutoGrid2()
        {
            ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
            ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            ChildMargin = new System.Windows.Thickness(3);
        }
    }
}
