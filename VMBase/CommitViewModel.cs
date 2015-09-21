using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Extensions;

namespace Tonic.MVVM
{
    /// <summary>
    /// Expose a commited property and a OnCommit event for view models that support commits. A CommitCommand is dynamically exposed on run time
    /// </summary>
    public class CommitViewModel : BaseViewModel, ICommitViewModel
    {
        /// <summary>
        /// Create a new CommitViewModel instance
        /// </summary>
        public CommitViewModel()
        {
            AddExtension(new CommandsExtension(this, new[] { nameof(Commit) }));
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
        /// Calls the try commit method and 
        /// </summary>
        public void Commit()
        {
            BeforeCommit();
            OnCommit?.Invoke(this, new EventArgs());
            hasBeenComited = true;
            RaisePropertyChanged(nameof(Committed));
        }

        /// <summary>
        /// Implement custom logic before the OnCommit event raises
        /// </summary>
        protected virtual void BeforeCommit() { }
    }
}
