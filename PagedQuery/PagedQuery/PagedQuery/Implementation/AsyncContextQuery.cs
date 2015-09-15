using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tonic.Patterns.PagedQuery.Implementation.Base;
namespace Tonic.Patterns.PagedQuery.Implementation
{
    /// <summary>
    /// Context query implementation with async item loading
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    internal class AsyncContextQuery<TContext, TIn, TOut>
        : BaseContextQuery<TContext, TIn, TOut>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private const int RequestCountToIgnore = 400;
        private const int LoadDelay = 200;

        #region Ctor
        private readonly TaskScheduler SyncThread;
        public AsyncContextQuery(Func<TContext> ContextCtor, Func<TContext, IQueryable<TIn>> QueryCtor, int pageSize, int pageCount, bool SupressContextDispose)
            : base(ContextCtor, QueryCtor, pageSize, pageCount, SupressContextDispose)
        {
            this.SyncThread = TaskScheduler.FromCurrentSynchronizationContext();
        }
        #endregion
        #region GetItem

        private readonly object SyncRoot = new object();
        /// <summary>
        /// Pages are lazily initialized
        /// </summary>
        private bool FirstPage = true;
        private async Task LoadItem(int index)
        {
            if (!FirstPage)
            {
                await Task.Delay(LoadDelay);
            }

            FirstPage = false;

            var a = Interlocked.Decrement(ref RequestedItems);
            if (a <= 0)
            {

                var NewPageIndex = (index / pageSize) * pageSize;

                var data = new TOut[pageSize];

                int count = await Task.Run(() =>
                {
                    lock (SyncRoot)
                    {
                        return ImmediateExecuteEnumerator((q) => q.Skip(NewPageIndex).Take(pageSize), data);
                    }
                }
                );

                var NewPage = paginator.AddPage(NewPageIndex);
                NewPage.Count = count;
                Array.Copy(data, NewPage.Items, count);

                RaiseCollectionChanged();
            }


        }

        private static int RequestedItems;
        protected override TOut GetItem(int index)
        {
            Pagination.Page<TOut> Page;

            if (paginator.TryGetPage(index, out Page))
            {
                return Page.Items[index - Page.Index];
            }
            else
            {
                if (RequestedItems < RequestCountToIgnore)
                {
                    Interlocked.Increment(ref RequestedItems);
                    var supressWarning = LoadItem(index);
                }
                return default(TOut);
            }

        }
        #endregion
        #region Count
        private async Task LoadCount()
        {
            this.count = await Task.Run(() => ((IQueryable<TOut>)this).Count());
            this.RaisePropertyChanged("Count");
            this.RaiseCollectionChanged();
        }

        private bool loadingCount;

        private int count = 1;
        public override int Count
        {
            get
            {
                if (!loadingCount)
                {
                    loadingCount = true;
                    //var supressWarning = LoadCount();
                    lock (SyncRoot)
                    {
                        count = ((IQueryable<TOut>)this).Count();
                    }
                }

                return count;
            }
        }

        protected override Composer.DeferredQueryBase<TIn, TNewOutput> CreateNewOutputInstance<TNewOutput>(bool Ordered)
        {
            if (Ordered)
                return new AsyncOrderedContextQuery<TContext, TIn, TNewOutput>(ContextCtor, QueryCtor, pageSize, pageCount, SupressContextDispose);
            else
                return new AsyncContextQuery<TContext, TIn, TNewOutput>(ContextCtor, QueryCtor, pageSize, pageCount, SupressContextDispose);
        }
        #endregion


        #region Notification events
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void RaiseCollectionChanged()
        {
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        private void RaiseItemChanged(int index, TOut ItemValue)
        {
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion
    }
}
