using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Exposes a commited and a commit event for view models that can notify when the commit operation was executed
    /// </summary>
    public interface ICommitViewModel
    {
        /// <summary>
        /// Raises when the commit has been executed succesfully
        /// </summary>
        event EventHandler OnCommit;
        /// <summary>
        /// Returns true if the view model has been commited succesfully
        /// </summary>
        bool Committed { get; }

        /// <summary>
        /// Returns true if the view model state enable commits
        /// </summary>
        bool CanCommit { get; }

        /// <summary>
        /// Raise the OnCommit event and set the HasBeenCommited property to true
        /// </summary>
        void Commit();
    }
}
