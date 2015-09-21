using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Extensions;

namespace Tonic.MVVM
{
    /// <summary>
    /// Expose model properties as dynamic properties.
    /// This is a light-weight CommitViewModel child with the added CommandsExtension and the ExposedModelExtension that implements the IModelViewModel and the ICommitViewModel interface, without any aditional logic
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExposedViewModel<T> : CommitViewModel, IModelViewModel<T>
    {
        /// <summary>
        /// Create a new instance for an exposed view model
        /// </summary>
        public ExposedViewModel()
        {
            AddExtension(new CommandsExtension(this));
            AddExtension(new ExposedModelExtension(this, nameof(Model), RaisePropertyChanged, SetError));
        }


        private T model;
        /// <summary>
        /// The model that exposes its properties
        /// </summary>
        public T Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
                RaisePropertyChanged(nameof(Model));
            }
        }


    }
}
