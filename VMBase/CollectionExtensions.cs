using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Provides helper collection methods
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Clear the collection and add all the specified items
        /// </summary>
        public static void Set<T>(this ICollection<T> x, IEnumerable<T> Items)
        {
            x.Clear();
            x.AddRange(Items);
        }

        /// <summary>
        /// Add all items to the collection
        /// </summary>
        public static void AddRange<T>(this ICollection<T> x, IEnumerable<T> Items)
        {
            foreach (var y in Items)
                x.Add(y);
        }
    }
}
