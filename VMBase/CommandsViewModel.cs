using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// A base view model with the CommandsExtension added
    /// </summary>
    public class CommandsViewModel : BaseViewModel
    {
        /// <summary>
        /// Create a new commands view model
        /// </summary>
        public CommandsViewModel ()
        {
            AddExtension(new Tonic.MVVM.Extensions.CommandsExtension(this));
        }
    }
}
