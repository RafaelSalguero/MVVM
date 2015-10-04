using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.UI
{
    /// <summary>
    /// A binding with the NotifyOnValidationError = true by default
    /// </summary>
    public class Binding : System.Windows.Data.Binding
    {
        public Binding() : this(null)
        {

        }

        public Binding(string Path) : base(Path)
        {
            NotifyOnValidationError = true;
        }
    }
}
