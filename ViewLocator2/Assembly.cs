using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Interfaces;

namespace Tonic.MVVM
{
    public class EntryAssemblyInfo : IAssembly
    {
        public DateTime Date
        {
            get
            {
                var As = System.Reflection.Assembly.GetEntryAssembly().GetName();

                return (new DateTime(2000, 1, 1)).AddDays(As.Version.Build).AddSeconds(As.Version.Revision * 2);
            }
        }

        public string Name
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            }
        }
    }
}
