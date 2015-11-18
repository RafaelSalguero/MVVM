using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Extensions;

namespace Tonic.MVVM
{
    /// <summary>
    /// Expose a commited property and a OnCommit event for view models that support commits. A CommitCommand is dynamically exposed on run time.
    /// The CanCommit property is calculated from the validation errors of the instance view model
    /// </summary>
    public class CommitViewModel : BaseViewModel, ICommitViewModel
    {
        /// <summary>
        /// Create a new CommitViewModel instance
        /// </summary>
        public CommitViewModel()
        {
            AddExtension(new CommandsExtension(this, new[] { Tuple.Create(nameof(Commit), nameof(CanCommit)) }));

        }

        private bool hasBeenComited;

        /// <summary>
        /// Returns true if the Commit method executed succesfully
        /// </summary>
        public bool Committed
        {
            get
            {
                return hasBeenComited;
            }
        }

        /// <summary>
        /// Raises after the Commit method executed succesfully
        /// </summary>
        public event EventHandler OnCommit;

        /// <summary>
        /// Returns true if the view model has no validation errors
        /// </summary>
        public virtual bool CanCommit
        {
            get
            {
                return !((INotifyDataErrorInfo)this).HasErrors;
            }
        }

        /// <summary>
        /// Calls the BeforeCommit method, raise the OnCommit event and change the Committed property to true
        /// </summary>
        public async Task Commit()
        {
            if (!CanCommit)
                throw new InvalidOperationException("CanCommit must be true");
            if (await BeforeCommit())
            {
                OnCommit?.Invoke(this, new EventArgs());
                hasBeenComited = true;
                RaisePropertyChanged(nameof(Committed));
            }
        }

        /// <summary>
        /// Implement custom logic before the OnCommit event raises. Returns false if the commit operation must be cancelled
        /// </summary>
        protected virtual async Task<bool> BeforeCommit() { return true; }
    }
}
