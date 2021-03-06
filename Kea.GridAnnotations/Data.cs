﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Kea.GridData
{
    /// <summary>
    /// Specify the column width in 1/96 inches. If this attribute is not specified, the column is fitted to content
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ColumnWidthAttribute : Attribute
    {
        readonly double width;

        /// <summary>
        /// Specify the column width in pixels
        /// </summary>
        /// <param name="width">Width in pixels</param>
        public ColumnWidthAttribute(double width)
        {
            this.width = width;
        }

        /// <summary>
        /// Column width in pixels
        /// </summary>
        public double Width
        {
            get { return width; }
        }
    }

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
        public DataColumn(string Property, string FriendlyName, string Format, double? ColumnWidth) : this(Property, FriendlyName)
        {
            this.Format = Format;
            this.Width = ColumnWidth;
        }


        public DataColumn(Func<object, object> PropertyGetter, string FriendlyName)
        {
            this.PropertyGetter = PropertyGetter;
            this.FriendlyName = FriendlyName;
        }

        public DataColumn(Func<object, object> PropertyGetter, string FriendlyName, string Format) : this(PropertyGetter, FriendlyName, Format, null)
        {

        }

        public DataColumn(Func<object, object> PropertyGetter, string FriendlyName, string Format, double? ColumnWidth)
        {
            this.PropertyGetter = PropertyGetter;
            this.FriendlyName = FriendlyName;
            this.Format = Format;
            this.Width = ColumnWidth;
        }


        /// <summary>
        /// Column friendly name that will appear to the user
        /// </summary>
        public string FriendlyName { get; private set; }
        /// <summary>
        /// Function that gets the value of a cell for a given object for this column
        /// </summary>
        public Func<object, object> PropertyGetter { get; private set; }
        /// <summary>
        /// String formatting. Only valid for cell values that implement the .ToString(string Format) method
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// Column width in 1/96 inch. Null if the column should be fitted to content
        /// </summary>
        public double? Width { get; private set; }


        public static Func<IEnumerable, int, object> GetIndexer(Type Type)
        {
            if (Type.IsSubClassOfGeneric(typeof(IReadOnlyList<>)))
            {
                //Access by the indexer property:
                var ElementType = Kea.ReflectionLinq.GetEnumerableType(Type);
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
        /// Exporta todas las propiedades públicas como columnas
        /// </summary>
        /// <param name="Collection"></param>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public static IEnumerable<DataColumn> FromData(IEnumerable Collection)
        {
            return FromData(Collection, P => true);
        }

        /// <summary>
        /// Exporta todas las propiedades públicas como columnas
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static IEnumerable<DataColumn> FromData(IEnumerable Collection, Func<string, bool> Predicate)
        {
            var Type = Kea.ReflectionLinq.GetEnumerableType(Collection);
            var FirstRow = Kea.ReflectionLinq.CallStatic(Collection, x => x.FirstOrDefault());

            var Properties = Type.GetProperties().Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null);

            var EAc = FastMember.TypeAccessor.Create(Type);
            foreach (var P in Properties.Where(x => Predicate(x.Name)))
            {
                ColumnListAttribute ColumnList;
                if ((ColumnList = P.GetCustomAttribute<ColumnListAttribute>()) != null)
                {
                    //Column list:
                    if (FirstRow != null)
                    {
                        var Data = (IEnumerable)P.GetValue(FirstRow);
                        var ElementType = Kea.ReflectionLinq.GetEnumerableType(P.PropertyType);
                        var Getter = GetIndexer(P.PropertyType);

                        int i = 0;
                        foreach (var C in Data)
                        {
                            var Ac = FastMember.TypeAccessor.Create(ElementType);
                            var Title = Ac[C, ColumnList.TitleProperty].ToString();

                            foreach (var Subcolumn in FromType(ElementType))
                            {
                                int localI = i;
                                yield return new DataColumn(o => Subcolumn.PropertyGetter(Getter((IEnumerable)EAc[o, P.Name], localI)), Title, Subcolumn.Format);
                            }
                            i++;
                        }
                    }
                }
                else
                {
                    var Format = P.GetCustomAttribute<FormatAttribute>()?.Format;
                    if (Format == null)
                    {
                        if (P.PropertyType == typeof(DateTime) || P.PropertyType == typeof(DateTime?))
                            Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    }
                    var Width = P.GetCustomAttribute<ColumnWidthAttribute>()?.Width;

                    yield return new DataColumn(P.Name, GetFriendlyName(P), Format, Width);
                }
            }
        }

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

        public static string ToString(object Value, string Format)
        {
            if (Value == null)
                return "";
            if (string.IsNullOrEmpty(Format))
                return Value.ToString();
            else
                return ((dynamic)Value).ToString(Format);
        }

        public static string[,] ToString(object[,] value, IReadOnlyList<string> formats)
        {
            var result = new string[value.GetLength(0), value.GetLength(1)];
            for (int y = 0; y < value.GetLength(0); y++)
            {
                for (int x = 0; x < value.GetLength(1); x++)
                {
                    result[y, x] = ToString(value[y, x], formats[x]);
                }
            }
            return result;
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
