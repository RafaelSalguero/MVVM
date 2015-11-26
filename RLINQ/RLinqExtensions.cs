using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.RLinqExtensions
{
    /// <summary>
    /// Expose RLinq as extension methods
    /// </summary>
    public static class RLinqExtensions
    {
        /// <summary>
        /// Calls the first LINQ method via reflection
        /// </summary>
        public static object First(this IQueryable Query)
        {
            return Tonic.RLinq.CallStatic(Query, x => x.First());
        }

        /// <summary>
        /// Calls the first LINQ method via reflection
        /// </summary>
        public static object First(this IEnumerable Query)
        {
            return Tonic.RLinq.CallStatic(Query, x => x.First());
        }

        /// <summary>
        /// Calls the where LINQ method via reflection
        /// </summary>
        public static IQueryable Where(this IQueryable Query, Expression Predicate)
        {
            return Tonic.RLinq.Where(Query, Predicate);
        }

        /// <summary>
        /// Calls the where LINQ method via reflection
        /// </summary>
        public static IEnumerable Where(this IEnumerable Query, Expression Predicate)
        {
            return Tonic.RLinq.Where(Query, Predicate);
        }

        /// <summary>
        /// Calls teh ToList LINQ method via reflection
        /// </summary>
        public static IEnumerable ToList(this IEnumerable Collection)
        {
            return Tonic.RLinq.ToList(Collection);
        }

    }
}
