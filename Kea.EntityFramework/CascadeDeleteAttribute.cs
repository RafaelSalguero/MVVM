using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.EntityFramework
{
    /// <summary>
    /// Especifica que una propiedad de navegación representa una llave foranea con CASCADE DELETE.
    /// Debed de usarse en conjunto con la convención CascadeDeleteAttributeConvention
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CascadeDeleteAttribute : Attribute { }
}
