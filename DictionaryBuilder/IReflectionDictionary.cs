using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.DictionaryBuilder
{
    public interface IProperties
    {
        object GetValue(string Key);
        void SetValue(string Key, object Value);
        Type GetValueType(string Key);
        IEnumerable<string> Keys { get; }
    }

    public class TupleProperties : IProperties
    {
        public TupleProperties(IEnumerable<Tuple<Type, string, object>> Properties)
        {
            this.Values = Properties.ToDictionary(x => x.Item2, x => x.Item3);
            this.Types = Properties.ToDictionary(x => x.Item2, x => x.Item1);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return Values.Keys;
            }
        }

        private readonly Dictionary<string, object> Values;
        private readonly Dictionary<string, Type> Types;

        public object GetValue(string Key)
        {
            return Values[Key];
        }

        public Type GetValueType(string Key)
        {
            return Types[Key];
        }

        public void SetValue(string Key, object Value)
        {
            Values[Key] = Value;
        }
    }

    public class TypeDescriptorProperties : IProperties
    {
        public TypeDescriptorProperties(object Instance)
        {
            this.Instance = Instance;
            this.properties = TypeDescriptor.GetProperties(Instance).Cast<PropertyDescriptor>();

        }
        readonly object Instance;
        IEnumerable<PropertyDescriptor> properties;

        public IEnumerable<string> Keys
        {
            get
            {
                return properties.Select(x => x.Name);
            }
        }

        public object GetValue(string Key)
        {
            return properties.First(x => x.Name == Key).GetValue(Instance);
        }

        public void SetValue(string Key, object Value)
        {
            properties.First(x => x.Name == Key).SetValue(Instance, Value);
        }

        public Type GetValueType(string Key)
        {
            return properties.First(x => x.Name == Key).PropertyType;
        }
    }
}
