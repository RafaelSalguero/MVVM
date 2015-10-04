using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Tonic.UI
{
    /// <summary>
    /// Expose simple typed properties to XAML for easied casting
    /// </summary>
    public class Cast : MarkupExtension
    {
        private object parameter;

        public int Int { set { parameter = value; } }
        public double Double { set { parameter = value; } }
        public float Float { set { parameter = value; } }
        public bool Bool { set { parameter = value; } }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return parameter;
        }
    }
}
