using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Tonic.Patterns.PagedQuery.Pagination
{
    /// <summary>
    /// Manages in-memory pages, max page size and page count
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PageCache<T>
    {
        /// <summary>
        /// Create a new page cache with the given page size and count
        /// </summary>
        /// <param name="PageSize">The max number of items that a page can have</param>
        /// <param name="PageCount">The max number of cached pages, if this limit is reached, older pages are overwritten by newly pages</param>
        public PageCache(int PageSize, int PageCount)
        {
            this.PageSize = PageSize;
            this.pageCount = PageCount;
        }
        public int PageSize
        {
            get;
            private set;
        }
        private readonly int pageCount;

        public int PageCount
        {
            get
            {
                return pageCount;
            }
        }

        /// <summary>
        /// Page circular buffer
        /// </summary>
        private Page<T>[] Pages;
        /// <summary>
        /// Current page index
        /// </summary>
        private int currentPage = 0;


        /// <summary>
        /// Checks if exists a page containing the given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool TryGetPage(int index, out Page<T> page)
        {
            if (Pages != null)
                for (int i = 0; i < Pages.Length; i++)
                {
                    if (Pages[i] != null && Pages[i].ContainsIndex(index))
                    {
                        page = Pages[i];
                        return true;
                    }
                }

            page = null;
            return false;
        }

        ///// <summary>
        ///// Add a new page to the page cache. This doesn't check if the page already exist, use the TryGetPage method before adding a page for retriving data from the cache. The method returns the added page
        ///// </summary>
        ///// <param name="index"></param>
        ///// <param name="data"></param>
        //[Obsolete("Use AddPage(index) and populate the internal array and count instead")]
        //public Page<T> AddPage(int index, IEnumerable<T> data)
        //{
        //    if (Pages == null) Pages = new Page<T>[PageCount];

        //    //if the current page is null, create a new page
        //    if (Pages[currentPage] == null)
        //    {
        //        Pages[currentPage] = new Page<T>(PageSize);
        //    }

        //    //Set the data to the current page, if the page already exist, its data will be overwritten
        //    Pages[currentPage].Set(index, data);
        //    var ret = Pages[currentPage];

        //    currentPage = currentPage + 1 == Pages.Length ? 0 : currentPage + 1;

        //    return ret;
        //}

        /// <summary>
        /// Add a new page to the page cache. This doesn't check if the page already exist, use the TryGetPage method before adding a page for retriving data from the cache. The method returns the added page
        /// </summary>
        /// <param name="index"></param>
        public Page<T> AddPage(int index)
        {
            if (Pages == null)
                Pages = new Page<T>[PageCount];

            //if the current page is null, create a new page
            if (Pages[currentPage] == null)
            {
                Pages[currentPage] = new Page<T>(PageSize);
            }

            //Set the data to the current page, if the page already exist, its data will be overwritten
            Pages[currentPage].Index = index;
            var ret = Pages[currentPage];

            currentPage = currentPage + 1 == Pages.Length ? 0 : currentPage + 1;

            return ret;
        }
    }
}
