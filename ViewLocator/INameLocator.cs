using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Located a resource by name
    /// </summary>
    public interface INameLocator
    {
        /// <summary>
        /// Get a resource by name, if the resource is not found, return null
        /// </summary>
        /// <param name="Name">The name of the resource</param>
        /// <returns>The resource</returns>
        object Get(string Name);
    }
}
