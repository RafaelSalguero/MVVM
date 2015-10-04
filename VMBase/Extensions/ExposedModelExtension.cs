using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM.Extensions
{
    /// <summary>
    /// Expose model properties as dynamic view model properties. Expose data errors from the model instance when a model property is changed, when the model instance is changed, all the model properties are notified for property changed
    /// </summary>
    public class ExposedModelExtension : IDynamicExtension
    {
        private Dictionary<string, PropertyInfo> modelProps = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// Model property
        /// </summary>
        private readonly PropertyInfo Prop;
        private readonly object instance;

        private readonly Action<string> RaisePropertyChanged;
        private readonly Action<string, string> SetError;

        /// <summary>
        /// Create a new exposed model extension without support for validation errors and model instance change property notification
        /// </summary>
        /// <param name="Instance">The view model instance</param>
        /// <param name="PropertyName">The view model property that contains the model</param>
        public ExposedModelExtension(object Instance, string PropertyName) : this(Instance, PropertyName, null) { }


        /// <summary>
        /// Create a new exposed model extension without support for validation errors
        /// </summary>
        /// <param name="Instance">The view model instance</param>
        /// <param name="PropertyName">The view model property that contains the model</param>
        /// <param name="RaisePropertyChanged">Raise the view model property changed event. Called when the model instance is changed once for every model property. If null the notification will not be done when the model instance is changed</param>
        public ExposedModelExtension(object Instance, string PropertyName, Action<string> RaisePropertyChanged) : this(Instance, Instance.GetType().GetProperty(PropertyName), RaisePropertyChanged, null) { }

        /// <summary>
        /// Create a new exposed model extension
        /// </summary>
        /// <param name="Instance">The view model instance</param>
        /// <param name="PropertyName">The view model property that contains the model</param>
        /// <param name="RaisePropertyChanged">Raise the view model property changed event. Called when the model instance is changed once for every model property. If null the notification will not be done when the model instance is changed</param>
        /// <param name="SetError">Sets a validation error on the view model. If null the model validation will not be reflected on the view model errors</param>
        public ExposedModelExtension(object Instance, string PropertyName, Action<string> RaisePropertyChanged, Action<string, string> SetError) : this(Instance, Instance.GetType().GetProperty(PropertyName), RaisePropertyChanged, SetError) { }

        /// <summary>
        /// Create a new exposed model extension
        /// </summary>
        /// <param name="Instance">The view model instance</param>
        /// <param name="Prop">The view model property that contains the model</param>
        /// <param name="RaisePropertyChanged">Raise the view model property changed event. Called when the model instance is changed once for every model property</param>
        /// <param name="SetError">Sets a validation error on the view model</param>
        public ExposedModelExtension(object Instance, PropertyInfo Prop, Action<string> RaisePropertyChanged, Action<string, string> SetError)
        {
            this.RaisePropertyChanged = RaisePropertyChanged;
            this.SetError = SetError;

            this.instance = Instance;
            this.Prop = Prop;
            foreach (var P in Prop.PropertyType.GetProperties())
            {
                modelProps.Add(P.Name, P);
            }

            var PC = Instance as INotifyPropertyChanged;

            if (PC != null)
                PC.PropertyChanged += (a, b) =>
               {
                   if (b.PropertyName == Prop.Name)
                       UpdateModel();

                   UpdateErrors();
               };
        }

        private object Model
        {
            get
            {
                return Prop.GetValue(instance);
            }
        }

        /// <summary>
        /// Raise property changed event for all model properties
        /// </summary>
        private void UpdateModel()
        {
            if (RaisePropertyChanged != null)
                foreach (var V in modelProps.Keys)
                    RaisePropertyChanged(V);
        }

        #region ErrorInfo
        private void UpdateErrors()
        {
            if (SetError != null)
            {
                IDataErrorInfo mod = Model as IDataErrorInfo;
                if (mod != null)
                {
                    foreach (var e in GetErrors(mod))
                    {
                        SetError(e.Key, e.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Get model errors when the model implements the IDataErrorInfo interface
        /// </summary>
        private static Dictionary<string, string> GetErrors(IDataErrorInfo Model)
        {
            var ret = new Dictionary<string, string>();
            foreach (var P in Model.GetType().GetProperties())
            {
                var Err = Model[P.Name];
                if (!string.IsNullOrEmpty(Err))
                {
                    ret.Add(P.Name, Err);
                }
                else
                    ret.Add(P.Name, null);
            }
            return ret;
        }
        #endregion

        #region Dynamic
        IEnumerable<string> IDynamicExtension.MemberNames
        {
            get
            {
                return modelProps.Keys;
            }
        }

        bool IDynamicExtension.CanRead(string PropertyName)
        {
            return modelProps[PropertyName].CanRead;
        }

        bool IDynamicExtension.CanWrite(string PropertyName)
        {
            return modelProps[PropertyName].CanWrite;
        }

        object IDynamicExtension.Get(string PropertyName)
        {
            var Model = Prop.GetValue(instance);
            return modelProps[PropertyName].GetValue(Model);
        }

        void IDynamicExtension.Set(string PropertyName, object Value)
        {
            //The property notificacion is done on the base view model
            var Model = Prop.GetValue(instance);
            modelProps[PropertyName].SetValue(Model, Value);
        }

        Type IDynamicExtension.GetPropertyType(string PropertyName)
        {
            return modelProps[PropertyName].PropertyType;
        }
        #endregion
    }
}
