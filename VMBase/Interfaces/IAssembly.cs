using System;
using System.Collections.Generic;
using System.Linq;
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
}
