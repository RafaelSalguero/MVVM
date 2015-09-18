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
    public interface IView
    {
        /// <summary>
        /// Shows a view given a view model.
        /// </summary>
        /// <param name="ViewModel">The view model to inject onto the view</param>
        void ShowDialog(object ViewModel);
    }

    /// <summary>
    /// Simple mock of the IView interface
    /// </summary>
    public class ViewMock : IView
    {
        /// <summary>
        /// A queue with the calls made to the ShowDialog method
        /// </summary>
        public Queue<object> ShowDialogCalls = new Queue<object>();

        void IView.ShowDialog(object ViewModel)
        {
            Console.WriteLine($"Show dialog {ViewModel}");
        }
    }
}
