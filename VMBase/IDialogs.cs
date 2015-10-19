using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Contains mockable methods for showing views from view models
    /// </summary>
    public interface IDialogs
    {
        /// <summary>
        /// Shows a model view given a view model.
        /// </summary>
        /// <param name="ViewModel">The view model to inject onto the view</param>
        void ShowDialog(object ViewModel);

        /// <summary>
        /// Shows a modeless view given a view model
        /// </summary>
        /// <param name="ViewModel">The view model to inject onto the view</param>
        void Show(object ViewModel);
    }

    /// <summary>
    /// Simple mock of the IView interface
    /// </summary>
    public class ViewMock : IDialogs
    {
        /// <summary>
        /// A queue with the calls made to the ShowDialog method
        /// </summary>
        public Queue<object> ShowDialogCalls = new Queue<object>();

        public void Show(object ViewModel)
        {
            Console.WriteLine($"Show {ViewModel}");
        }

        void IDialogs.ShowDialog(object ViewModel)
        {
            Console.WriteLine($"Show dialog {ViewModel}");
        }
    }
}
