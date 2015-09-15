using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.Pagination
{
    /// <summary>
    /// Enumerate an IPaginator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PaginatorEnumerator<T> : IEnumerator<T>
    {
        public PaginatorEnumerator(IPaginator<T> Paginator)
        {
            this.Paginator = Paginator;
        }

        private readonly IPaginator<T> Paginator;

        private T current = default(T);
        public T Current
        {
            get { return current; }
        }

        public void Dispose()
        {
            //Doesn't dispose the orignal query
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        int index = 0;
        public bool MoveNext()
        {
            //Get a page:
            var Page = Paginator.GetPage(index);
            if (Page.Count < Page.Items.Length)
            {
                //This page is the last
                if (index == Page.Count + Page.Index)
                    return false;
            }
            current = Page.Items[index - Page.Index];
            index++;
            return true;
        }

        public void Reset()
        {
            //By design
            throw new NotSupportedException();
        }
    }
}
