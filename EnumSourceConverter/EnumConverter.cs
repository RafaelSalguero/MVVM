using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Reflection;
namespace Tonic.UI
{
    public class EnumConverter : IValueConverter
    {
        [ThreadStatic]
        static Dictionary<Type, List<Tuple<string, object>>> enumNames = new Dictionary<Type, List<Tuple<string, object>>>();


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

        private static List<Tuple<string, object>> GetEnumNames(Type Type)
        {
            if (enumNames.ContainsKey(Type))
                return enumNames[Type];

            var values = Enum.GetValues(Type);
            var result = new List<Tuple<string, object>>();
            foreach (var V in values)
            {
                var Att = V.GetType().GetField(V.ToString()).GetCustomAttribute<DescriptionAttribute>();
                if (Att == null)
                    result.Add(Tuple.Create(FixName(V.ToString()), V));
                else
                    result.Add(Tuple.Create(Att.Description, V));
            }
            enumNames.Add(Type, result);
            return result;
        }

        public static string EnumToString(object Value)
        {
            if (Value == null) return null;
            var Type = Value.GetType();
            var Names = GetEnumNames(Type);
            return Names.Where(x => object.Equals(x.Item2, Value)).FirstOrDefault()?.Item1 ?? Value.ToString();
        }

        public static object StringToEnum(string Value, Type Type)
        {
            var Names = GetEnumNames(Type);
            return Names.Where(x => x.Item1 == Value).FirstOrDefault()?.Item2;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EnumValue.Create(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumValue)
            {
                return ((EnumValue)value).Value;
            }
            else
                return StringToEnum(value.ToString(), targetType);

        }
    }
}
