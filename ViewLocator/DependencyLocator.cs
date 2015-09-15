using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Base class for view model locators with dependency injection
    /// </summary>
    public abstract class DependencyLocator : INameLocator
    {
        private readonly IServiceProvider ioc;
        /// <summary>
        /// Dependency container
        /// </summary>
        public IServiceProvider IoC
        {
            get
            {
                return ioc;
            }
        }

        /// <summary>
        /// Create a new dependency locator, given its dependency container
        /// </summary>
        /// <param name="IoC"></param>
        public DependencyLocator(IServiceProvider IoC)
        {
            this.ioc = IoC;
        }

        object INameLocator.Get(string Name)
        {
            var Type = GetViewModelType(Name);
            if (Type == null) return null;
            if (ioc == null)
            {
                return Activator.CreateInstance(Type);
            }
            else
            {
                return ioc.GetService(Type);
            }
        }

        /// <summary>
        /// Gets the view model type related to this name
        /// </summary>
        protected abstract Type GetViewModelType(string Name);
    }
}
