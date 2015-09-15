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

    public class ViewMock : IView
    {
        public void ShowDialog(object ViewModel)
        {
            Console.WriteLine($"Show dialog {ViewModel}");
        }
    }
}
