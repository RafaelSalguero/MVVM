using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.Pagination
{
    internal interface IPaginator<T>
    {
        /// <summary>
        /// Gets a page at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Page<T> GetPage(int index);
    }
}
