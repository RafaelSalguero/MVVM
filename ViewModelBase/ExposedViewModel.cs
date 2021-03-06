﻿using System;
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



        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!base.TryGetMember(binder, out result))
            {
                if (Model == null)
                    throw new NullReferenceException($"Property '{binder.Name}' The model is null");

                PropertyInfo P;
                if (modelProps.TryGetValue(binder.Name, out P))
                {
                    result = P.GetValue(model);
                    return true;
                }
                else return false;
            }
            else
                return true;
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
                        //Si es asi, establece la propiedad y dispara la notificacion de propiedad
                        P.SetValue(model, value);
                        RaisePropertyChanged(P.Name);

                        //Actualiza los errores, solo aplica si el modelo implementa IDataErrorInfo
                        UpdateErrors();
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
