using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// This interface ensures at compile time that a class implement the IModelViewModel(T) interface. Should never be implemented directly 
    /// </summary>
    public interface IModelViewModel
    {
        /// <summary>
        /// Gets or sets the model associated with this view model
        /// </summary>
        object Model { get; set; }

        /// <summary>
        /// Gets the type of the model
        /// </summary>
        Type ModelType { get; }
    }

    /// <summary>
    /// A view model that exposes a model property that can be changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IModelViewModel<T> : IModelViewModel
    {
        /// <summary>
        /// Gets or sets the model associated with this view model
        /// </summary>
        new T Model { get; set; }
    }


}
