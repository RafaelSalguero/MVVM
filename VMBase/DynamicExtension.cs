using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Extension that intercepts dynamic get and sets from view models
    /// </summary>
    public interface IDynamicExtension
    {
        /// <summary>
        /// Gets a value given a property name
        /// </summary>
        /// <param name="PropertyName">A property name that exists in MemberNames</param>
        object Get(string PropertyName);

        /// <summary>
        /// Sets a value to the given property
        /// </summary>
        /// <param name="PropertyName">A property name that exists in MemberNames</param>
        /// <param name="Value">The new value of the property</param>
        void Set(string PropertyName, object Value);

        /// <summary>
        /// Returns true if the given property can be readed
        /// </summary>
        /// <param name="PropertyName">A property name that exist in MemberNames</param>
        bool CanRead(string PropertyName);

        /// <summary>
        /// Returns true if the given property can be written
        /// </summary>
        /// <param name="PropertyName">A property name that exist in MemberNames</param>
        bool CanWrite(string PropertyName);

        /// <summary>
        /// Get all valid dynamic member names
        /// </summary>
        IEnumerable<string> MemberNames { get; }

        /// <summary>
        /// Gets the type of a property given its name
        /// </summary>
        /// <param name="PropertyName">The name of the property</param>
        /// <returns>The property type</returns>
        Type GetPropertyType(string PropertyName);
    }
}
