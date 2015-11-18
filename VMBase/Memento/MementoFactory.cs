using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Tonic.MVVM
{
    /// <summary>
    /// Provides a method for creating dynamic mementos
    /// </summary>
    public static class MementoFactory
    {
        [ThreadStatic]
        private static ProxyGenerator generator = new ProxyGenerator();

        class ProxyInterceptor : IInterceptor
        {
            /// <summary>
            /// Contains all properties that should be copied back to the original model
            /// </summary>
            public readonly HashSet<string> copyProperties;


            public readonly object Source;
            public ProxyInterceptor(object Source, HashSet<string> copyProperties)
            {
                this.copyProperties = copyProperties;
                this.Source = Source;

                //Agrega todas las propiedades no virtuales a modified properties
                foreach (var P in Source.GetType().GetProperties().Where(x => !x.GetGetMethod().IsVirtual && x.CanRead && x.CanWrite).Select(x => x.Name))
                    copyProperties.Add(P);
            }


            Dictionary<string, object> virtualPropertiesValues = new Dictionary<string, object>();

            public void Intercept(IInvocation invocation)
            {
                bool isGet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("get_");
                bool isSet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("set_");
                var propName = (isGet || isSet) ? invocation.Method.Name.Substring("get_".Length) : null;

                if (isSet)
                {
                    var Value = invocation.Arguments[0];
                    virtualPropertiesValues[propName] = Value;
                    copyProperties.Add(propName);
                    return;
                }

                object retValue;
                if (isGet)
                {
                    if (virtualPropertiesValues.TryGetValue(propName, out retValue))
                    {
                        invocation.ReturnValue = retValue;
                        return;
                    }
                }

                object RetValue = invocation.Method.Invoke(Source, invocation.Arguments);

                if (isGet)
                    virtualPropertiesValues.Add(propName, RetValue);

                invocation.ReturnValue = RetValue;
            }
        }


        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <param name="Instance">Instance to copy</param>
        /// <param name="CopyProperties">Contains all properties that should be copied back to the original view model when the change is commited</param>
        /// <returns></returns>
        public static object Lazy(Type Type, object Instance, out IEnumerable<string> CopyProperties)
        {
            var Method = typeof(MementoFactory).GetMethods()
                .Where(
                x => x.Name == nameof(Lazy) &&
                x.IsGenericMethodDefinition &&
                x.GetParameters().Length == 1
                ).Single().MakeGenericMethod(Type);

            dynamic R = Method.Invoke(null, new object[] { Instance });
            CopyProperties = R.Item2;
            return R.Item1;
        }

        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <typeparam name="T">Type to proxy</typeparam>
        /// <param name="Instance">Instance to copy</param>
        /// <param name="CopyProperties">Contains all properties that should be copied back to the original view model when the change is commited</param>
        /// <returns></returns>
        public static T Lazy<T>(T Instance, out IEnumerable<string> CopyProperties)
            where T : class
        {
            var cP = new HashSet<string>();
            CopyProperties = cP;
            var ret = (T)generator.CreateClassProxy(typeof(T), new ProxyInterceptor(Instance, cP));

            //Copy all non virtual properties:
            foreach (var P in typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite && !x.GetGetMethod().IsVirtual))
            {
                var value = P.GetValue(Instance);
                P.SetValue(ret, value);
            }
            return ret;
        }

        /// <summary>
        /// Create an instance of type T that have a copy of all non-virtual properties, and property proxies for virtual ones.
        /// This allows to manage a proxy for a memento without compromising lazy-loading of ORMs
        /// </summary>
        /// <typeparam name="T">Type to proxy</typeparam>
        /// <param name="Instance">Instance to copy</param>
        /// <returns></returns>
        public static Tuple<T, IEnumerable<string>> Lazy<T>(T Instance)
            where T : class
        {
            IEnumerable<string> copy;
            T result = Lazy(Instance, out copy);
            return Tuple.Create(result, copy);
        }
    }

}
