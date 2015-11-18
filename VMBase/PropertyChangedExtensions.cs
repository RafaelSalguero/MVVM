using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Property changed extensions
    /// </summary>
    public static class PropertyChangedExtensions
    {
        /// <summary>
        /// Execute the given action when the given property has changed
        /// </summary>
        /// <param name="Notifier"></param>
        /// <param name="PropertyName">The property to listen</param>
        /// <param name="Action">The action to execute</param>
        public static void OnPropertyChanged(this INotifyPropertyChanged Notifier, string PropertyName, Action Action)
        {
            Notifier.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == PropertyName)
                    Action();
            };
        }
    }
}
