using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Base view model, provides property changed notification and validation
    /// </summary>
    public abstract class BaseViewModel : DynamicObject, INotifyPropertyChanged, INotifyDataErrorInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Raise the property changed event
        /// </summary>
        /// <param name="Name"></param>
        protected void RaisePropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }

        #region DynamicObject

        [Serializable]
        protected class DynamicGetException : Exception
        {
            public DynamicGetException() { }
            public DynamicGetException(string message) : base(message) { }
            public DynamicGetException(string message, Exception inner) : base(message, inner) { }
            protected DynamicGetException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context)
            { }
        }
        protected abstract object DynamicGet(string PropertyName);
        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                result = DynamicGet(binder.Name);
                return true;
            }
            catch (DynamicGetException)
            {
                result = null;
                return false;
            }
        }
        #endregion 

        #region GetErrors
        private Dictionary<string, List<object>> errors = new Dictionary<string, List<object>>();

        /// <summary>
        /// Sets an error message to a property name. If the value is null, the error is cleared
        /// </summary>
        /// <param name="PropertyName">The property</param>
        /// <param name="Error">The error message</param>
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
                    List.Add(Error);
                    errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(PropertyName));
                }
            }
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
        /// Adds a validation error to a property
        /// </summary>
        /// <param name="PropertyName">The property</param>
        /// <param name="Error">The error message</param>
        protected void AddError(string PropertyName, object Error)
        {
            GetErrors(PropertyName).Add(Error);
            errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(PropertyName));
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
