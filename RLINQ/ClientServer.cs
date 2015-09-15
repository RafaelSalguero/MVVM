using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    public static class ClientServer
    {
        public static IQueryable<T> OrderServerFirst<T>(this IQueryable<T> Query)
        {
            return Query;
        }


    }
}
