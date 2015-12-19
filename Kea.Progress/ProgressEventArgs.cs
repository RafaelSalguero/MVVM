using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs ( double Progress) 
        {
            this.Progress = Progress;
        }
        public double Progress { get; private set; }
    }
}
