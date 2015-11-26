using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tonic;
namespace Kea.GridData
{
    /// <summary>
    /// Especifica un formato de columna para la definicion autogenerada de las columnas
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class FormatAttribute : Attribute
    {
        readonly string format;

        public FormatAttribute(string format)
        {
            this.format = format;
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
    /// Especifica que la enumeración en el objeto se deberá de representar como múltiples columnas
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ColumnListAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string titleProperty;

        public ColumnListAttribute(string titleProperty, bool subtitle)
        {
            this.titleProperty = titleProperty;
            this.Subtitle = subtitle;
        }

        /// <summary>
        /// Especifica el titulo de la columna, que sera tomado de el primer renglon de la información
        /// </summary>
        public string TitleProperty
        {
            get { return titleProperty; }
        }

        /// <summary>
        /// Especifica si se utilizaran los nombres de las propiedades de los objetos de esta colección como subtitulos
        /// </summary>
        public bool Subtitle
        {
            get; private set;
        }
    }



    /// <summary>
    /// Data column specification used for serializing objects to data rows
    /// </summary>
    public class DataColumn
    {
        public DataColumn(string Property, string FriendlyName)
        {
            this.PropertyGetter = (o) =>
            {
                var Ac = FastMember.ObjectAccessor.Create(o);
                return Ac[Property];
            };
            this.FriendlyName = FriendlyName;
        }

        public DataColumn(string Property, string FriendlyName, string Format) : this(Property, FriendlyName)
        {
            this.Format = Format;
        }


        public DataColumn(Func<object, object> PropertyGetter, string FriendlyName)
        {
            this.PropertyGetter = PropertyGetter;
            this.FriendlyName = FriendlyName;
        }

        public string FriendlyName { get; private set; }

        public Func<object, object> PropertyGetter { get; private set; }
        public string Format { get; private set; }

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

        public static Func<IEnumerable, int, object> GetIndexer(Type Type)
        {
            if (Type.IsSubClassOfGeneric(typeof(IReadOnlyList<>)))
            {
                //Access by the indexer property:
                var ElementType = RLinq.GetEnumerableType(Type);
                var Interface = typeof(IReadOnlyList<>).MakeGenericType(ElementType);
                var PropertyIndexer = Interface.GetProperties().Where(x => x.GetIndexParameters().Length > 0).Single();


                var Collection = Expression.Parameter(typeof(IEnumerable));
                var Index = Expression.Parameter(typeof(int));
                var Body = Expression.Convert(Expression.MakeIndex(Expression.Convert(Collection, Interface), PropertyIndexer, new[] { Index }), typeof(object));

                var Expr = Expression.Lambda<Func<IEnumerable, int, object>>(Body, Collection, Index);
                return Expr.Compile();
            }
            else
            {
                return (o, i) => o.Cast<object>().Take(i).First();
            }
        }

        public static IEnumerable<DataColumn> FromType(Type ElementType)
        {
            return FromData(Array.CreateInstance(ElementType, 0));
        }

        /// <summary>
        /// Export all public properties as data columns
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static IEnumerable<DataColumn> FromData(IEnumerable Collection)
        {
            var Type = RLinq.GetEnumerableType(Collection);
            var FirstRow = RLinq.CallStatic(Collection, x => x.FirstOrDefault());

            var Properties = Type.GetProperties().Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null);

            var EAc = FastMember.TypeAccessor.Create(Type);
            foreach (var P in Properties)
            {
                ColumnListAttribute ColumnList;
                if ((ColumnList = P.GetCustomAttribute<ColumnListAttribute>()) != null)
                {
                    //Column list:
                    if (FirstRow != null)
                    {
                        var Data = (IEnumerable)P.GetValue(FirstRow);
                        var ElementType = RLinq.GetEnumerableType(P.PropertyType);
                        var Getter = GetIndexer(P.PropertyType);

                        int i = 0;
                        foreach (var C in Data)
                        {
                            var Ac = FastMember.TypeAccessor.Create(ElementType);
                            var Title = Ac[C, ColumnList.TitleProperty].ToString();

                            foreach (var Subcolumn in FromType(ElementType))
                            {
                                int localI = i;
                                yield return new DataColumn(o => Subcolumn.PropertyGetter(Getter((IEnumerable)EAc[o, P.Name], localI)), Title);
                            }
                            i++;
                        }
                    }
                }
                else
                {
                    yield return new DataColumn(P.Name, GetFriendlyName(P));
                }
            }
        }
    }

    /// <summary>
    /// Provee métodos para exportar objetos a matrices de cadenas
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// Exporta un objecto a una fila de cadenas
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> ToRow(object o, DataColumn[] Columns)
        {

            int x = 0;
            foreach (var c in Columns)
            {
                var cell = c.PropertyGetter(o);
                yield return cell;
            }
        }

        /// <summary>
        /// Exporta una colección de objetos a una matriz de cadenas
        /// </summary>
        /// <param name="Objects">Objetos a exportar</param>
        /// <param name="Columns">Definición de las columnas</param>
        public static object[,] ToGrid(IEnumerable<object> Objects, IEnumerable<DataColumn> Columns)
        {

            var cols = Columns.ToArray();
            var count = Objects.Count();

            var ret = new object[count, cols.Length];

            int x, y = 0;
            foreach (var o in Objects)
            {
                x = 0;
                foreach (var cell in ToRow(o, cols))
                {
                    ret[y, x] = cell;
                    x++;
                }
                y++;
            }

            return ret;
        }
    }
}
