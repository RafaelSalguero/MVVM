using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Tonic.UI
{
    public class EnumBinding : MarkupExtension
    {
        public EnumBinding(string Path)
        {
            this.path = Path;
            this.binding = new Binding(Path);
            binding.Converter = new EnumConverter();
        }

        public EnumBinding() : this("") { }
        private readonly string path;
        private readonly Binding binding;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return binding.ProvideValue(serviceProvider);
        }
    }
}
