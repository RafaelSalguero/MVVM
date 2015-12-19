using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace Tonic.MVVM.Dialogs
{
    /// <summary>
    /// Shows a progress bar for long-running operations
    /// </summary>
    public class ProgressViewModel : CommandsViewModel, IProgress<double>
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
        /// Progress value
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Idle
        /// </summary>
        [DependsOn(nameof(Value))]
        public bool Idle
        {
            get
            {
                return Value == null;
            }
        }

        /// <summary>
        /// Close the progress view dialog
        /// </summary>
        public void Close()
        {
            Closed = true;
        }

        /// <summary>
        /// Change the message
        /// </summary>
        public void ReportProgress(string Message, double? Value)
        {
            this.Message = Message;
            this.Value = Value;
        }

        void IProgress<double>.Report(double value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Set the message
        /// </summary>
        /// <param name="Message"></param>
        public void Log(string Message)
        {
            this.Message = Message;
        }
    }
}
