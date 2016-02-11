using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM.Interfaces
{
    /// <summary>
    /// Assembly info interface
    /// </summary>
    public interface IAssembly
    {
        /// <summary>
        /// Assembly name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Assembly version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Compilation date
        /// </summary>
        DateTime Date { get; }
    }


    public class AssemblyMock : IAssembly
    {
        DateTime IAssembly.Date => new DateTime(2016, 01, 26);

        string IAssembly.Name => "Tonic.MVVM";

        string IAssembly.Version => "1.0.5214.1254";
    }

    public class BaseAssemblyInfo : IAssembly
    {
        AssemblyName As;
        public BaseAssemblyInfo(AssemblyName A)
        {
            this.As = A;
        }

        public DateTime Date => (new DateTime(2000, 1, 1)).AddDays(As.Version.Build).AddSeconds(As.Version.Revision * 2);
        public string Name => As.Name;
        public string Version => As.Version.ToString();

    }
}
