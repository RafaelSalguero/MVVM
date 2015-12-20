using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.AsyncCrud
{
    public static class DbContextExtensions
    {
        public static TElement FirstOrDefaultAsync<TElement>(this DbSet<TElement> Set, object PrimaryKey)
            where TElement : class
        {
            Set.find
        }
    }
}

