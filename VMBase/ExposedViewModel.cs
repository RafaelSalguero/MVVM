using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Expose model properties as dynamic properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExposedViewModel<T> : CommandsViewModel, IModelViewModel<T>
    {
        private Dictionary<string, PropertyInfo> modelProps = new Dictionary<string, PropertyInfo>();
        public ExposedViewModel()
        {
            var type = GetType();
            foreach (var P in typeof(T).GetProperties())
            {
                //Si el usuario ya implementa esta propiedad, se ignora:
                if (type.GetProperty(P.Name) != null)
                    continue;

                modelProps.Add(P.Name, P);
            }

            this.PropertyChanged += (a, b) => UpdateErrors();
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
            }
        }


        protected override object DynamicGet(string PropertyName)
        {
            try
            {
                return base.DynamicGet(PropertyName);
            }
            catch (DynamicGetException)
            {
                if (Model == null)
                    throw new NullReferenceException($"Property '{PropertyName}' The model is null");

                PropertyInfo P;
                if (modelProps.TryGetValue(PropertyName, out P))
                {
                    return P.GetValue(model);
                }
                else
                    throw new DynamicGetException();
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!base.TrySetMember(binder, value))
            {
                PropertyInfo P;
                if (modelProps.TryGetValue(binder.Name, out P))
                {
                    //Revisa si el nuevo valor es diferente al anterior
                    var LastValue = P.GetValue(model);
                    if (!object.Equals(LastValue, value))
                    {
                        //Si es asi, establece la propiedad
                        P.SetValue(model, value);

                        //La notificacion de propiedades la debe de implementar el modelo
                        //Al asignar la propiedad Model, si la nueva instancia implementa INotifyPropertyChanged, esta clase se subscribe al evento PropertyChanged
                        //Al deteectar cambios de propiedad tambien se actualizan los errores
                    }
                    return true;
                }
                else return false;
            }
            else
                return true;
        }
        #region ErrorInfo
        private void UpdateErrors()
        {
            IDataErrorInfo mod = Model as IDataErrorInfo;
            if (mod != null)
            {
                foreach (var e in GetErrors(mod))
                    SetError(e.Key, e.Value);
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
    }
}
