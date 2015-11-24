using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM
{
    /// <summary>
    /// Gets and sets values by string keys
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Set a value by its key
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        void Set(string Key, string Value);
        /// <summary>
        /// Gets a value by its key, returns null if not found
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        string Get(string Value);
    }

    /// <summary>
    /// IConfig extensions
    /// </summary>
    public static class Config
    {
        private static Guid singletonGuid = new Guid("705DB7AE-B07B-4691-9754-7C2BAC21C93A");
        /// <summary>
        /// Sets a json value by its key
        /// </summary>
        public static void Set(this IConfig Config, object Key, object Value)
        {
            Config.Set(Newtonsoft.Json.JsonConvert.SerializeObject(Key), Newtonsoft.Json.JsonConvert.SerializeObject(Value));
        }

        /// <summary>
        /// Gets a value by its json key, return null or default(T) if not found
        /// </summary>
        public static T Get<T>(this IConfig Config, object Key)
        {
            var value = Config.Get(Newtonsoft.Json.JsonConvert.SerializeObject(Key));
            return value != null ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value) : default(T);
        }

        static string getSingletonKey(Type Type)
        {
            return singletonGuid.ToString() + Type.FullName;
        }

        /// <summary>
        /// Sets a singleton value using its type as a key
        /// </summary>
        /// <param name="Singleton">The value to save</param>
        public static void Set(this IConfig Config, object Singleton)
        {
            Config.Set(getSingletonKey(Singleton.GetType()), Singleton);
        }

        /// <summary>
        /// Gets a singleton value using the given type as a key, returns null of default(T) if not found
        /// </summary>
        public static T Get<T>(this IConfig Config)
        {
            return Config.Get<T>(getSingletonKey(typeof(T)));
        }
    }
}
