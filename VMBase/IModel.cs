using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// A view model that exposes a model property that can be changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IModelViewModel<T>
    {
        /// <summary>
        /// Gets or sets the model associated with this view model
        /// </summary>
        T Model { get; set; }
    }

    
}
