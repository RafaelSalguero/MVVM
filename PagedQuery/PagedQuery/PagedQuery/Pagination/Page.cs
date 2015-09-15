using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.Pagination
{
    /// <summary>
    /// Contains a range of in-memory values from a paged query.
    /// </summary>
    internal class Page<T>
    {
        /// <summary>
        /// Create a new page object containing the given data.
        /// </summary>
        public Page(int capacity)
        {
            this.Items = new T[capacity];
            this.Index = -1;
        }


        /// <summary>
        /// Copy the data of the enumeration onto the queue, if the data contains more elements that the capacity of the page, an exception is thrown
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Data"></param>
        public void Set(int index, IEnumerable<T> Data)
        {
            int i = 0;
            foreach (var D in Data)
            {
                Items[i] = D;
                i++;
            }
            this.Index = index;
            this.Count = i;
        }

        /// <summary>
        /// The number of items on this page
        /// </summary>
        public int Count { get;  set; }
        public int Index { get;  set; }
        public T[] Items { get; private set; }

        public bool ContainsIndex(int i)
        {
            return i >= this.Index && i < (this.Index + this.Items.Length);
        }
    }


}
