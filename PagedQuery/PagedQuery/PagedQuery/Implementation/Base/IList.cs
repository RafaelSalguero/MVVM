using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.Pagination;

namespace Tonic.Patterns.PagedQuery.Implementation.Base
{
    internal partial class BaseContextQuery<TContext, TIn, TOut> : Composer.DeferredQueryBase<TIn, TOut>, IList<TOut>, IList
    {
        #region IList
        //Not supported
        int IList<TOut>.IndexOf(TOut item)
        {
            return -1;
        }

        void IList<TOut>.Insert(int index, TOut item)
        {
            throw new NotImplementedException();
        }

        void IList<TOut>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }


        void ICollection<TOut>.Add(TOut item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TOut>.Clear()
        {
            throw new NotImplementedException();
        }

        //Not supported
        bool ICollection<TOut>.Contains(TOut item)
        {
            return false;
        }

        void ICollection<TOut>.CopyTo(TOut[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (var It in this)
            {
                array[i] = It;
                i++;
            }
        }


        bool ICollection<TOut>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<TOut>.Remove(TOut item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<TOut> IEnumerable<TOut>.GetEnumerator()
        {
            return new PaginatorEnumerator<TOut>(this);
        }

        public bool ImmediateEnumeratorMode = true;

        System.Collections.IEnumerator GetItemGetterEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (ImmediateEnumeratorMode)
            {
                return ((IEnumerable<TOut>)this).GetEnumerator();
            }
            else
            {
                //Returns async IEnumerator, this method is used by WPF when the collection is initializaed by the first time
                return GetItemGetterEnumerator();

            }
            //return null;//((IEnumerable<TOut>)this).GetEnumerator();
        }
        #endregion

        #region IList
        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            return ((IList<TOut>)this).Contains((TOut)value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList<TOut>)this).IndexOf((TOut)value);

        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        bool IList.IsFixedSize
        {
            get { return true; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TOut this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Item getter
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract TOut GetItem(int index);


        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList<TOut>)this).CopyTo((TOut[])array, index);
        }

        public abstract int Count
        {
            get;
        }

        int ICollection.Count
        {
            get
            {
                return Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }


        private object syncRoot = new object();
        object ICollection.SyncRoot
        {
            get { return syncRoot; }
        }
        #endregion
    }
}
