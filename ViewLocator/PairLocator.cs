using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Tonic.MVVM
{


    /// <summary>
    /// A view model locator binds view data context to view model instances
    /// </summary>
    public class PairLocator : DependencyLocator
    {


        private List<VVMPair> items = new List<VVMPair>();
        private const string ViewPostfix = "View";
        private const string ViewModelPostfix = "ViewModel";

        /// <summary>
        /// Create a new empty pair locator
        /// </summary>
        /// <param name="DependencyContainer"></param>
        public PairLocator(IServiceProvider DependencyContainer) : base(DependencyContainer)
        { }

        /// <summary>
        /// Create a view model locator from two dummy types folling the view view model naming convention
        /// </summary>
        /// <param name="ViewAssemblyDummyType">Type at the root of the namespace of the views. Can be null if only view model names are needed</param>
        /// <param name="ViewModelAssemblyDummyType">Type at the root of the namespace of the view models. Can be null if only view names are needed</param>
        /// <param name="DependencyContainer"></param>
        public PairLocator(Type ViewAssemblyDummyType, Type ViewModelAssemblyDummyType, IServiceProvider DependencyContainer)
            : this(ViewAssemblyDummyType?.Assembly, ViewAssemblyDummyType?.Namespace, ViewModelAssemblyDummyType?.Assembly, ViewModelAssemblyDummyType?.Namespace, DependencyContainer)
        { }

        /// <summary>
        /// Create a view model locator from a view assembly and a view model assembly, both following naming convention
        /// </summary>
        /// <param name="ViewModelNamespace">Root namespace of the view models</param>
        /// <param name="ViewModels">Assembly to look for view models. Can be null if only view names are needed</param>
        /// <param name="ViewNamespace">Root namespace of the views</param>
        /// <param name="Views">Assembly to look for views. Can be null if only view model names are needed</param>
        /// <param name="DependencyContainer">Dependency container for resolving view model instantiation</param>
        public PairLocator(Assembly Views, string ViewNamespace, Assembly ViewModels, string ViewModelNamespace, IServiceProvider DependencyContainer) : base(DependencyContainer)
        {
            AddByConvention(Views, ViewNamespace, ViewModels, ViewModelNamespace);
        }

        /// <summary>
        /// Add a new family of pairs that follow the naming convention
        /// </summary>
        /// <param name="ViewModelNamespace">Root namespace of the view models</param>
        /// <param name="ViewModels">Assembly to look for view models. Can be null if only view names are needed</param>
        /// <param name="ViewNamespace">Root namespace of the views</param>
        /// <param name="Views">Assembly to look for views. Can be null if only view model names are needed</param>
        public void AddByConvention(Assembly Views, string ViewNamespace, Assembly ViewModels, string ViewModelNamespace)
        {
            foreach (var VM in ConventionLocator.Locate(Views, ViewNamespace, ViewModels, ViewModelNamespace))
                items.Add(VM);
        }

        /// <summary>
        /// Add a new name-view model pair
        /// </summary>
        public void Add(Type ViewModelType, string Name)
        {
            items.Add(new VVMPair(null, ViewModelType, Name));
        }

        /// <summary>
        /// Add a new name-view model pair
        /// </summary>
        /// <param name="Pair"></param>
        public void Add(VVMPair Pair)
        {
            items.Add(Pair);
        }

        /// <summary>
        /// Add a new view model type that follows the naming convention
        /// </summary>
        /// <param name="ViewModelType"></param>
        public void Add(Type ViewModelType)
        {
            if (!ViewModelType.Name.EndsWith(ViewModelPostfix))
                throw new ArgumentException($"The view model type {ViewModelType.Name} should end with {ViewModelPostfix }");

            Add(ViewModelType, ViewModelType.Name.Substring(0, ViewModelType.Name.Length - ViewModelPostfix.Length));
        }

        /// <summary>
        /// Gets a view model type by name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        protected override Type GetViewModelType(string Name)
        {
            return items.FirstOrDefault(x => x.Name == Name)?.ViewModel;
        }
    }
}
