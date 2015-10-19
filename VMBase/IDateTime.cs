using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Provides the current date time in an abstract way that can be unit-tested
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets the current context datetime
        /// </summary>
        DateTime Now { get; }
    }

    /// <summary>
    /// Returns the current date time
    /// </summary>
    public class SystemDateTime : IDateTimeProvider
    {
        /// <summary>
        /// Gets the current system date time
        /// </summary>
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Gets the specified date time
    /// </summary>
    public class MockDateTime : IDateTimeProvider
    {
        /// <summary>
        /// Create a mock date time that returns the specified date time
        /// </summary>
        /// <param name="Now"></param>
        public MockDateTime(DateTime Now)
        {
            this.Now = Now;
        }

        /// <summary>
        /// Create a mock date time that returns 26-Jan-2015 date
        /// </summary>
        public MockDateTime()
        {
            Now = new DateTime(2015, 1, 26);
        }

        /// <summary>
        /// Gets or sets the resulting date
        /// </summary>
        public DateTime Now
        {
            get; set;
        }
    }
}
