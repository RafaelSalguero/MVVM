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

    public static class ConventionLocator
    {
        private const string ViewPostfix = "View";
        private const string ViewModelPostfix = "ViewModel";

        public static IEnumerable<VVMPair> Locate(Type ViewAssemblyDummyType, Type ViewModelAssemblyDummyType)
        {
            return Locate(ViewAssemblyDummyType.Assembly, ViewAssemblyDummyType.Namespace, ViewModelAssemblyDummyType.Assembly, ViewModelAssemblyDummyType.Namespace);
        }

        private static string GetName(string fullName, string ns, string post) => fullName.Substring(ns.Length + 1, fullName.Length - post.Length - ns.Length - 1);

        /// <summary>
        /// Gets a new family of pairs that follow the naming convention
        /// </summary>
        public static IEnumerable<VVMPair> Locate(Assembly Views, string ViewNamespace, Assembly ViewModels, string ViewModelNamespace)
        {


            var vTypes = Views.GetTypes().Where(x => x.FullName.StartsWith(ViewNamespace + ".") && x.FullName.EndsWith(ViewPostfix))
                           .ToLookup(x => GetName(x.FullName, ViewNamespace, ViewPostfix));

            var vmTypes = ViewModels.GetTypes().Where(x => x.FullName.StartsWith(ViewModelNamespace + ".") && x.FullName.EndsWith(ViewModelPostfix))
                .ToLookup(x => GetName(x.FullName, ViewModelNamespace, ViewModelPostfix));

            var ret = new List<VVMPair>();

            HashSet<Tuple<Type, Type>> usedPairs = new HashSet<Tuple<Type, Type>>();

            foreach (var Group in vTypes)
                foreach (var View in Group)
                    foreach (var ViewModel in vmTypes[Group.Key])
                    {
                        ret.Add(new VVMPair(View, ViewModel, Group.Key));
                        usedPairs.Add(Tuple.Create(View, ViewModel));
                    }

            foreach (var Group in vmTypes)
                foreach (var ViewModel in Group)
                    foreach (var View in vTypes[Group.Key])
                        if (!usedPairs.Contains(Tuple.Create(View, ViewModel)))
                            ret.Add(new VVMPair(View, ViewModel, Group.Key));


            return ret;
        }

    }
}
