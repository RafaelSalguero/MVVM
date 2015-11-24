using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM.Dialogs
{
    /// <summary>
    /// Shows a progress bar for long-running operations
    /// </summary>
    public class ProgressViewModel : CommandsViewModel
    {
        /// <summary>
        /// Window title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Current message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// True if the dialog should be closed. Call the Close() method for setting this property
        /// </summary>
        public bool Closed { get; private set; }

        /// <summary>
        /// Close the progress view dialog
        /// </summary>
        public void Close ()
        {
            Closed = true;
        }

        /// <summary>
        /// Change the message
        /// </summary>
        public void ReportProgress(string Message)
        {
            this.Message = Message;
        }
    }
}
