using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tonic.UI
{
    class GetTypeEnumConverter : IValueConverter
    {
        public BindingExpression expr;
        public bool UseDescription;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (expr == null) return null;

            var Type = PropertyPathHelper.GetSourcePropertyType(expr.ResolvedSource, expr.ResolvedSourcePropertyName);
            if (UseDescription)
                return EnumSource.GetEnumValues(Type).Select(x => EnumValue.Create(x)).ToArray();
            else
                return EnumSource.GetEnumValues(Type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
