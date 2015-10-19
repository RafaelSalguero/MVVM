using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Extensions;

namespace Tonic.MVVM
{

    /// <summary>
    /// Base view model, provides property changed notification, validation and support for dynamic extensions
    /// </summary>
    public abstract class BaseViewModel : DynamicExtensionContainer, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// Notify when a view model property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Create a new instance of the BaseViewModel, with support for validation, change notification and dynamic extensions
        /// </summary>
        public BaseViewModel() : base((sender, e) => ((BaseViewModel)sender).RaisePropertyChanged(e.PropertyName))
        {
        }


        /// <summary>
        /// Raise the property changed event
        /// </summary>
        /// <param name="Name"></param>
        protected void RaisePropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }


        #region GetErrors
        private Dictionary<string, List<object>> errors = new Dictionary<string, List<object>>();

        /// <summary>
        /// Set a property validation error
        /// </summary>
        /// <param name="PropertyName">The property name</param>
        /// <param name="Error">The error to set, null to remove all validation errors from the given property</param>
        protected void SetError(string PropertyName, object Error)
        {
            if (Error == null)
            {
                ClearError(PropertyName);
            }
            else
            {
                var List = GetErrors(PropertyName);
                bool Update = List.Count != 1 || !object.Equals(List[0], Error);

                if (Update)
                {
                    List.Clear();
                    AddError(PropertyName, Error);
                }
            }
        }

        /// <summary>
        /// Add an error to this property
        /// </summary>
        /// <param name="PropertyName">The property name</param>
        /// <param name="Error">The error to add</param>

        protected void AddError(string PropertyName, object Error)
        {
            if (Error == null)
                throw new ArgumentException(nameof(Error));
            var List = GetErrors(PropertyName);

            List.Add(Error);
            errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Clear the validation error for this property
        /// </summary>
        /// <param name="PropertyName">The property name</param>
        protected void ClearError(string PropertyName)
        {
            var List = GetErrors(PropertyName);
            if (List.Count != 0)
            {
                List.Clear();
                errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(PropertyName));
            }
        }

        /// <summary>
        /// Gets a list of errors for this property
        /// </summary>
        private List<object> GetErrors(string PropertyName)
        {
            List<object> ret;
            if (!errors.TryGetValue(PropertyName, out ret))
            {
                ret = new List<object>();
                errors.Add(PropertyName, ret);
            }
            return ret;
        }

        bool INotifyDataErrorInfo.HasErrors
        {
            get
            {
                return errors.Values.Any(x => x.Any());
            }
        }

        private EventHandler<DataErrorsChangedEventArgs> errorsChanged;
        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add
            {
                errorsChanged += value;
            }
            remove
            {
                errorsChanged -= value;
            }
        }
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return GetErrors(propertyName);
        }

        #endregion
    }
}
