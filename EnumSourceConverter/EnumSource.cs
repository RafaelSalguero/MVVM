using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Tonic.UI
{
    /// <summary>
    /// Return all enum value for a given enum type or a binding to a property with an enum Type
    /// Ussage: 
    /// </summary>
    public class EnumSource : MarkupExtension
    {
        private readonly Type _type;
        private readonly Binding _binding;

        [ThreadStatic]
        static Dictionary<Type, object[]> cache = new Dictionary<Type, object[]>();

        public bool Friendly { get; set; } = true;

        public EnumSource(object value)
        {
            if (value is Type)
            {
                var type = (Type)value;
                if (!type.IsEnum)
                    throw new ArgumentException("EnumToItemsSource requieres that the given type is a non-nullable enum");
                _type = type;
            }
            else if (value is Binding)
            {
                _binding = (Binding)value;
            }
            else if (value is String)
            {
                _binding = new Binding((string)value);
            }
            else
                throw new ArgumentException("Value must be an enum type, a Binding or an string");
        }


        private Type GetPropertyType(IServiceProvider serviceProvider)
        {
            //provider of target object and it's property
            var targetProvider = (IProvideValueTarget)serviceProvider
                .GetService(typeof(IProvideValueTarget));
            if (targetProvider.TargetProperty is DependencyProperty)
            {
                return ((DependencyProperty)targetProvider.TargetProperty).PropertyType;
            }

            return targetProvider.TargetProperty.GetType();
        }

        static readonly DependencyProperty BindingProperty =
     DependencyProperty.RegisterAttached(
         "Binding",
         typeof(Type),
         typeof(EnumSource),
         new PropertyMetadata());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_type == null)
            {
                //provider of target object and it's property
                var targetProvider = (IProvideValueTarget)serviceProvider
                    .GetService(typeof(IProvideValueTarget));
                var target = (FrameworkElement)targetProvider.TargetObject;

                var B = new Binding(_binding.Path.Path);
                B.Mode = BindingMode.TwoWay;
                var conv = new GetTypeEnumConverter();
                conv.UseDescription = Friendly;
                B.Converter = conv;

                var r = (BindingExpression)B.ProvideValue(serviceProvider);
                conv.expr = r;
                return r;
            }
            else
            {
                if (Friendly)
                    return GetEnumValues(_type).Select(x => EnumValue.Create(x));
                else
                    return GetEnumValues(_type);
            }
        }

        public static IEnumerable<object> GetEnumValues(Type Type)
        {
            object[] result;
            if (!cache.TryGetValue(Type, out result))
            {
                result = Enum.GetValues(Type).Cast<object>().ToArray();
                cache.Add(Type, result);
            }

            return result;
        }
    }
}
