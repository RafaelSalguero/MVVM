using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM.Dialogs
{
    /// <summary>
    /// File dialog view model
    /// </summary>
    public class FileViewModel : CommitViewModel
    {
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string FilePath { get; set; }

        public bool IsOpen { get; set; }
    }
}
