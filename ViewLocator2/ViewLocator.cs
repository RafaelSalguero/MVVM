using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Tonic.MVVM;
using Tonic.MVVM.Dialogs;

namespace Tonic.MVVM
{
    /// <summary>
    /// Contains a view model predicate and a view constructor, if the predicate pass, the given view constructor is related to the view model instance
    /// </summary>
    public class ViewViewModelDependency
    {
        /// <summary>
        /// Create a new model dependency
        /// </summary>
        /// <param name="ViewConstructor">The view factory</param>
        /// <param name="ViewModelPredicate">A delegate that test if a view model instance can be paired with the given view</param>
        public ViewViewModelDependency(Func<FrameworkElement> ViewConstructor, Func<object, bool> ViewModelPredicate)
        {
            this.ViewModelPredicate = ViewModelPredicate;
            this.ViewConstructor = ViewConstructor;
        }

        /// <summary>
        /// Create a dependency from a view type and a view model type
        /// </summary>
        public ViewViewModelDependency Create(Type View, Type ViewModel)
        {
            return new ViewViewModelDependency(() => (FrameworkElement)Activator.CreateInstance(View), x => ViewModel.IsAssignableFrom(x.GetType()));
        }


        /// <summary>
        /// Create a dependency from a view factory and a view model type
        /// </summary>
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

    /// <summary>
    /// Provides an implementation of the IDialogs interface with support for view factory methods
    /// </summary>
    public class ViewLocator : IDialogs
    {
        private readonly List<ViewViewModelDependency> dependencies = new List<ViewViewModelDependency>();

        public ViewLocator()
        {
            Add(typeof(Views.ProgressView), typeof(Tonic.MVVM.Dialogs.ProgressViewModel));
        }

        /// <summary>
        /// Add a dependency between a view constructor and a view predicate
        /// </summary>
        /// <param name="Dependency"></param>
        public void Add(ViewViewModelDependency Dependency)
        {
            dependencies.Add(Dependency);
        }

        public void Add(Type View, Type ViewModel)
        {
            Add(new VVMPair(View, ViewModel, ViewModel.Name));
        }

        /// <summary>
        /// Add a dependency between a view type and a view model type
        /// </summary>
        /// <param name="Dependency"></param>
        public void Add(VVMPair Dependency)
        {
            dependencies.Add(new ViewViewModelDependency(() => (FrameworkElement)Activator.CreateInstance(Dependency.View), o => o.GetType() == Dependency.ViewModel));
        }

        private FrameworkElement CreateView(object ViewModel)
        {
            var D = dependencies.LastOrDefault(x => x.ViewModelPredicate(ViewModel));
            var View = D.ViewConstructor();
            View.DataContext = ViewModel;
            return View;
        }

        private Window CreateWindow(object ViewModel)
        {
            var V = CreateView(ViewModel);
            if (!(V is Window))
                throw new NotSupportedException("Only Window views are supported");

            return (Window)V;
        }

        private void ShowFileDialog(FileViewModel Vm)
        {
            if (Vm.IsOpen)
            {
                var D = new OpenFileDialog();
                D.Title = Vm.Title;
                D.Filter = Vm.Extensions;
                D.FileName = Vm.FilePath;

                if (D.ShowDialog() == true)
                {
                    Vm.FilePath = D.FileName;
                    Vm.Commit();
                }
            }
            else
            {
                var D = new SaveFileDialog();
                D.Title = Vm.Title;
                D.Filter = Vm.Extensions;
                D.FileName = Vm.FilePath;
                D.AddExtension = true;

                if (D.ShowDialog() == true)
                {
                    Vm.FilePath = D.FileName;
                    Vm.Commit();
                }
            }
        }

        private void ShowMessageDialog(MessageViewModel Vm)
        {
            MessageBox.Show(Vm.Message, Vm.Title);
        }

        /// <summary>
        /// Shows the view which have a dependency on this view model
        /// </summary>
        /// <param name="ViewModel">The view model to attach to the new view</param>
        public void ShowDialog(object ViewModel)
        {
            if (ViewModel is FileViewModel)
            {
                ShowFileDialog((FileViewModel)ViewModel);
            }
            else if (ViewModel is MessageViewModel)
            {
                ShowMessageDialog((MessageViewModel)ViewModel);
            }
            else if (ViewModel is string)
            {
                ShowDialog(new MessageViewModel { Message = (string)ViewModel });
            }
            else
            {
                var Window = CreateWindow(ViewModel);
                Window.ShowDialog();
            }
        }



        /// <summary>
        /// Shows the view which have a dependency on this view model
        /// </summary>
        /// <param name="ViewModel">The view model to attach to the new view</param>
        public void Show(object ViewModel)
        {
            var Window = CreateWindow(ViewModel);
            Window.Show();
        }
    }
}
