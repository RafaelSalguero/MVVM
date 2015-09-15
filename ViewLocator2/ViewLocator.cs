using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tonic.MVVM;

namespace ViewLocator2
{
    /// <summary>
    /// Contains a view model predicate and a view constructor, if the predicate pass, the given view constructor is related to the view model instance
    /// </summary>
    public class ViewViewModelDependency
    {
        public ViewViewModelDependency(Func<FrameworkElement> ViewConstructor, Func<object, bool> ViewModelPredicate)
        {
            this.ViewModelPredicate = ViewModelPredicate;
            this.ViewConstructor = ViewConstructor;
        }

        public ViewViewModelDependency Create(Type View, Type ViewModel)
        {
            return new ViewViewModelDependency(() => (FrameworkElement)Activator.CreateInstance(View), x => ViewModel.IsAssignableFrom(x.GetType()));
        }

        public ViewViewModelDependency Create<TViewModel>(Func<FrameworkElement> ViewConstructor)
        {
            return new ViewViewModelDependency(ViewConstructor, x => typeof(TViewModel).IsAssignableFrom(x.GetType()));
        }

        /// <summary>
        /// Returns true if this dependency matches a view model instance
        /// </summary>
        public Func<object, bool> ViewModelPredicate { get; private set; }

        /// <summary>
        /// Returns a new view instance
        /// </summary>
        public Func<FrameworkElement> ViewConstructor { get; private set; }
    }

    public class ViewLocator : IView
    {
        private List<ViewViewModelDependency> dependencies;
        public void Add(ViewViewModelDependency Dependency)
        {
            dependencies.Add(Dependency);
        }


        private FrameworkElement CreateView(object ViewModel)
        {
            var D = dependencies.FirstOrDefault(x => x.ViewModelPredicate(ViewModel));
            var View = D.ViewConstructor();
            View.DataContext = ViewModel;
            return View;
        }

        public void ShowDialog(object ViewModel)
        {
            var V = CreateView(ViewModel);
            if (!(V is Window))
                throw new NotSupportedException("Only Window views are supported");

            var Window = (Window)V;
            Window.ShowDialog();
        }
    }
}
