using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// A view/viewmodel types pair
    /// </summary>
    public class VVMPair
    {
        public VVMPair(Type View, Type ViewModel, string Name)
        {
            this.View = View;
            this.ViewModel = ViewModel;
            this.Name = Name;
        }

        /// <summary>
        /// Convention based name
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// The view model type
        /// </summary>
        public Type ViewModel { get; private set; }

        /// <summary>
        /// The view type
        /// </summary>
        public Type View { get; private set; }
    }

    /// <summary>
    /// Pairs all view classes that ends with View with view model classes that ends with ViewModel
    /// </summary>
    public static class ConventionLocator
    {
        private const string ViewPostfix = "View";
        private const string ViewModelPostfix = "ViewModel";

        /// <summary>
        /// Get a new family of pairs that follow the naming convention
        /// </summary>
        /// <param name="ViewAssemblyDummyType">Type at the root of the namespace of the views. Can be null if only view model names are needed</param>
        /// <param name="ViewModelAssemblyDummyType">Type at the root of the namespace of the view models. Can be null if only view names are needed</param>
        /// <returns></returns>
        public static IEnumerable<VVMPair> Locate(Type ViewAssemblyDummyType, Type ViewModelAssemblyDummyType)
        {
            return Locate(ViewAssemblyDummyType?.Assembly, ViewAssemblyDummyType?.Namespace, ViewModelAssemblyDummyType?.Assembly, ViewModelAssemblyDummyType?.Namespace);
        }

        private static string GetName(string fullName, string ns, string post) => fullName.Substring(ns.Length + 1, fullName.Length - post.Length - ns.Length - 1);

        /// <summary>
        /// Gets a new family of pairs that follow the naming convention
        /// </summary>
        /// <param name="ViewModelNamespace">Root namespace of the view models</param>
        /// <param name="ViewModels">Assembly to look for view models. Can be null if only view names are needed</param>
        /// <param name="ViewNamespace">Root namespace of the views</param>
        /// <param name="Views">Assembly to look for views. Can be null if only view model names are needed</param>
        public static IEnumerable<VVMPair> Locate(Assembly Views, string ViewNamespace, Assembly ViewModels, string ViewModelNamespace)
        {


            var vTypes = (Views?.GetTypes().Where(x => x.FullName.StartsWith(ViewNamespace + ".") && x.FullName.EndsWith(ViewPostfix)) ?? new Type[0])
                           .ToLookup(x => GetName(x.FullName, ViewNamespace, ViewPostfix));

            var vmTypes = (ViewModels?.GetTypes().Where(x => x.FullName.StartsWith(ViewModelNamespace + ".") && x.FullName.EndsWith(ViewModelPostfix)) ?? new Type[0])
                .ToLookup(x => GetName(x.FullName, ViewModelNamespace, ViewModelPostfix));

            var ret = new List<VVMPair>();

            HashSet<Tuple<Type, Type>> usedPairs = new HashSet<Tuple<Type, Type>>();

            foreach (var Group in vTypes)
            {
                foreach (var View in Group)
                {
                    if (vmTypes[Group.Key].Any())
                        foreach (var ViewModel in vmTypes[Group.Key])
                        {
                            ret.Add(new VVMPair(View, ViewModel, Group.Key));
                            usedPairs.Add(Tuple.Create(View, ViewModel));

                        }
                    else
                        ret.Add(new VVMPair(View, null, Group.Key));
                }
            }

            foreach (var Group in vmTypes)
            {
                foreach (var ViewModel in Group)
                {
                    if (vTypes[Group.Key].Any())
                        foreach (var View in vTypes[Group.Key])
                        {
                            if (!usedPairs.Contains(Tuple.Create(View, ViewModel)))
                                ret.Add(new VVMPair(View, ViewModel, Group.Key));
                        }
                    else
                        ret.Add(new VVMPair(null, ViewModel, Group.Key));
                }
            }


            return ret;
        }

    }
}
