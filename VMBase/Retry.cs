using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Contains methods for retrying failing operations
    /// </summary>
    public static class Retry
    {
        /// <summary>
        /// Retry once the given action if the first time throws an exception. Equivalent to Do(Action, 1)
        /// </summary>
        public static void Once(Action Action)
        {
            Do(Action, 1);
        }

        /// <summary>
        /// Retry the given action if it throws an exception the number of specified times
        /// </summary>
        /// <param name="Action">The action to execute and retry if fails</param>
        /// <param name="Count">The number of action retries, 0 for executing the action only once</param>
        public static void Do(Action Action, int Count)
        {
            Do(() =>
           {
               Action();
               return 0;
           }, Count);
        }

        /// <summary>
        /// Retry once the given action if the first time throws an exception. Equivalent to Do(Action, 1)
        /// </summary>
        public static T Once<T>(Func<T> Action)
        {
            return Do(Action, 1);
        }

        /// <summary>
        /// Retry the given action if it throws an exception the number of specified times
        /// </summary>
        /// <param name="Action">The action to execute and retry if fails</param>
        /// <param name="Count">The number of action retries, 0 for executing the action only once</param>
        public static T Do<T>(Func<T> Action, int Count)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return Action();
                }
                catch (Exception)
                {
                    if (retryCount >= Count)
                        throw;
                    else
                        retryCount++;
                }
            }
            throw new Exception("Unreachable");
        }
    }
}
