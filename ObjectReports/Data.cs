using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Excel
{
    /// <summary>
    /// Specify the string format of a property in an excel report
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class FormatAttribute : Attribute
    {
        readonly string format;

        public FormatAttribute(string positionalString)
        {
            this.format = positionalString;
        }

        public string Format
        {
            get { return format; }
        }
    }

    /// <summary>
    /// Specify that this property will be excluded on an excel report
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute()
        {
        }
    }

    /// <summary>
    /// Data column specification used for serializing objects to data rows
    /// </summary>
    public class DataColumn
    {
        public DataColumn(string Property, string FriendlyName, Func<object, string> Converter)
        {
            this.Converter = Converter;
            this.Property = Property;
            this.FriendlyName = FriendlyName;
        }

        public DataColumn(string Property, string FriendlyName, string Format) : this(Property, FriendlyName, x => ((dynamic)x).ToString(Format))
        {
        }

        public DataColumn(string Property, string FriendlyName) : this(Property, FriendlyName, x => x.ToString())
        {

        }

        public Func<object, string> Converter { get; private set; }
        public string Property { get; private set; }
        public string FriendlyName { get; private set; }

        /// <summary>
        /// Add whitespaces to capitalization changes
        /// </summary>
        /// <returns></returns>
        static string FixName(string Name)
        {
            if (string.IsNullOrEmpty(Name)) return "";

            if (Name.Length >= 2 && Name.StartsWith("_") && char.IsDigit(Name[1]))
            {
                Name = Name.Substring(1);
            }

            StringBuilder B = new StringBuilder();
            B.Append(Name[0]);

            for (int i = 1; i < Name.Length; i++)
            {
                var last = Name[i - 1];
                var current = Name[i];

                if (char.IsUpper(current) && !char.IsUpper(last) || char.IsDigit(current) && !char.IsDigit(last))
                    B.Append(' ');
                B.Append(char.ToLower(current));
            }
            return B.ToString();
        }

        static string GetFriendlyName(PropertyInfo P)
        {
            var desc = P.GetCustomAttribute<DescriptionAttribute>();
            if (desc != null) return desc.Description;
            else
            {
                return FixName(P.Name);
            }
        }

        static string DefaultConverter(object Value, PropertyInfo P)
        {
            if (object.Equals(null, Value))
                return "";
            var type = Value.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return ((dynamic)Value).Value;

            var Format = P.GetCustomAttribute<FormatAttribute>();
            if (Format != null)
                return ((dynamic)Value).ToString(Format.Format);
            else
                return Value.ToString();
        }

        /// <summary>
        /// Export all public properties as data columns
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static IEnumerable<DataColumn> FromType(Type Type)
        {
            return Type.GetProperties().Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null).Select(x => new DataColumn(x.Name, GetFriendlyName(x), o => DefaultConverter(o, x))).ToArray();
        }
    }

    public static class Data
    {
        public static string[,] ToGrid(IEnumerable<object> Objects, IEnumerable<DataColumn> Columns)
        {

            var cols = Columns.ToArray();
            var count = Objects.Count();

            var ret = new string[count, cols.Length];

            int x, y = 0;
            foreach (var o in Objects)
            {
                var Ac = FastMember.ObjectAccessor.Create(o);
                x = 0;
                foreach (var c in Columns)
                {
                    var cell = c.Converter(Ac[c.Property]);
                    ret[y, x] = cell;
                    x++;
                }
                y++;
            }

            return ret;
        }
    }
}
