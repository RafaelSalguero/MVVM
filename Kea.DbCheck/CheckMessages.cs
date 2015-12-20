using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.DbCheck
{
    public enum DbCheckMessages
    {
        /// <summary>
        /// The column is defined on the model but it isn't found on the database
        /// </summary>
        ColumnDoesntExist = 1,

        /// <summary>
        /// The column name pass the foreign key name convention but it isn't a foreign key on the database
        /// </summary>
        ColumnForeignKeyNameConvention = 2,

        /// <summary>
        /// The column is defined as a foreign key on the model but it isn't defined as is on the database
        /// </summary>
        ColumnForeignKeyOnModel = 3,

        /// <summary>
        /// The table is defined on the model but not on the database
        /// </summary>
        TableDoesntExists = 4,

        /// <summary>
        /// The column type on the model is diferent on the database
        /// </summary>
        ColumnTypeMismatch = 5,

        /// <summary>
        /// The column is defined as nullable 
        /// </summary>
        ColumnNullableOnModel = 6,
    }
    public static class CheckMessages
    {
        public static Dictionary<DbCheckMessages, string> GetMessages()
        {
            var r = new Dictionary<DbCheckMessages, string>();

            r.Add(DbCheckMessages.ColumnDoesntExist, "The column {0} from table {1} is defined on the model but not on the database");
            r.Add(DbCheckMessages.ColumnForeignKeyNameConvention, "The column {0} from table {1} pass the foreign key name convention but it isn't defined as a foreign key");

            return r;
        }

    }
}
