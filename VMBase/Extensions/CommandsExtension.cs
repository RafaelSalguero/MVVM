using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tonic.MVVM.Extensions
{
    /// <summary>
    /// Expose public methods as commands. Public properties with the method name + "Enabled" will be used as the source for the CanExecute command property
    /// </summary>
    public class CommandsExtension : IDynamicExtension
    {

        /// <summary>
        /// A pair of a method and the related CanExecute property for that method
        /// </summary>
        public struct MethodCanExecutePair
        {
            /// <summary>
            /// Create a new method-property pair
            /// </summary>
            public MethodCanExecutePair(MethodInfo Method, string CanExecuteProperty)
            {
                this.Method = Method;
                this.CanExecuteProperty = CanExecuteProperty;
            }

            /// <summary>
            /// The method that will be executed by the command
            /// </summary>
            public MethodInfo Method { get; private set; }

            /// <summary>
            /// The CanExecute source property. Null if the method can always execute
            /// </summary>
            public string CanExecuteProperty { get; private set; }
        }

        private Dictionary<string, DelegateCommand> commands = new Dictionary<string, DelegateCommand>();



        private static IEnumerable<MethodInfo> getCommandMethods(object instance, IEnumerable<Type> exclude)
        {
            var Type = instance.GetType();

            var CompleteExclude = exclude.Concat(new Type[] { typeof(BaseViewModel) });
            if (Type.BaseType != null)
            {
                CompleteExclude = CompleteExclude.Concat(new Type[] { Type.BaseType });
            }
            HashSet<MethodInfo> excluded = new HashSet<MethodInfo>(CompleteExclude.SelectMany(x => x.GetMethods()));

            return Type.GetMethods().Where(x =>
            {
                var r = !x.IsSpecialName && !excluded.Any(y => y.Name == x.Name && x.DeclaringType == y.DeclaringType);
                var p = x.GetParameters();

                return r && p.Length <= 1;
            });
        }

        /// <summary>
        /// Expose public 0 and 1 parameter methods as command properties, with the 'Command' postfix. Base class methods are excluded
        /// </summary>
        /// <param name="Instance">The instance of the type that contains the methods</param>
        public CommandsExtension(object Instance) : this(Instance, getCommandMethods(Instance, new Type[0]))
        { }

        /// <summary>
        /// Expose public 0 and 1 parameter methods as command properties, with the 'Command' postfix
        /// </summary>
        /// <param name="Instance">The instance of the type that contains the methods</param>
        /// <param name="ExcludeMethodsFrom">Exclude methods from this types, BaseViewModel methods are excluded by default</param>
        public CommandsExtension(object Instance, IEnumerable<Type> ExcludeMethodsFrom) : this(Instance, getCommandMethods(Instance, ExcludeMethodsFrom))
        { }

        /// <summary>
        /// Expose the given methods as command properties with the 'Command' postfix
        /// </summary>
        /// <param name="Instance">The instance of the type that contains the methods</param>
        /// <param name="MethodNames">The given method names to expose</param>
        public CommandsExtension(object Instance, IEnumerable<string> MethodNames) : this(Instance, Instance.GetType().GetMethods().Where(x => MethodNames.Contains(x.Name)))
        {

        }

        private static IEnumerable<MethodCanExecutePair> fromNames(object Instance, IEnumerable<Tuple<string, string>> names)
        {
            var Methods = Instance.GetType().GetMethods();
            return names.SelectMany(x => Methods.Where(y => y.Name == x.Item1).Select(y => new MethodCanExecutePair(y, x.Item2)));
        }

        /// <summary>
        /// Expose the given methods as command properties with the 'Command' postfix
        /// </summary>
        /// <param name="Instance">The instance of the type that contains the methods</param>
        /// <param name="MethodCanExecuteNames">The given pairs of method and CanExecute property names to expose</param>
        public CommandsExtension(object Instance, IEnumerable<Tuple<string, string>> MethodCanExecuteNames) : this(Instance, fromNames(Instance, MethodCanExecuteNames))
        {

        }

        /// <summary>
        /// Expose the given methods as command properties with the 'Command' postfix, with paired CanExecute properties that have the 'Enabled' postfix
        /// </summary>
        public CommandsExtension(object Instance, IEnumerable<MethodInfo> Methods) : this(Instance, Methods.Select(x => new MethodCanExecutePair(x, x.Name + "Enabled")))
        {
        }

        /// <summary>
        /// Expose the given methods as command properties with the 'Command' postfix
        /// </summary>
        /// <param name="Instance">The instance of the type that contains the methods</param>
        /// <param name="Methods">The given methods to expose</param>
        public CommandsExtension(object Instance, IEnumerable<MethodCanExecutePair> Methods)
        {
            //Llena el diccionario de comandos a partir de la lista de methodos
            var InstanceType = Instance.GetType();

            //Relaciona los method info con los delegate commands. 
            //Esto para poder encontrar que comando le corresponde cuando se encuentre que una CanExecute property ha cambiado
            var inverseDic = new Dictionary<MethodInfo, DelegateCommand>();

            foreach (var MP in Methods)
            {
                var M = MP.Method;

                var CommandName = M.Name + "Command";
                //Si el miembro command ya existe, lo ignora:
                if (InstanceType.GetProperty(CommandName) != null)
                    continue;

                var accesor = FastMember.ObjectAccessor.Create(Instance);

                //El GetCanExecute parte del CanExecuteProperty, se usa el FastMember porque las propiedaes pueden ser dinámicas
                Func<object, bool> GetCanExecute = (parameter) =>
                 {
                     if (string.IsNullOrEmpty(MP.CanExecuteProperty)) return true;
                     try
                     {
                         return (bool)accesor[MP.CanExecuteProperty];
                     }
                     catch (Exception ex)
                     {
                         return true;
                     }
                 };

                var P = M.GetParameters();

                DelegateCommand Command;
                switch (P.Length)
                {
                    case 0:
                        Command = new DelegateCommand((o) => M.Invoke(Instance, new object[0]), GetCanExecute);
                        break;
                    case 1:
                        Command = new DelegateCommand((o) => M.Invoke(Instance, new object[] { o }), GetCanExecute);
                        break;
                    default:
                        throw new ArgumentException($"The command for the method '{M.Name}' can't be exposed because it has too many parameters ({P.Length})");
                }
                commands.Add(CommandName, Command);
                inverseDic.Add(M, Command);
            }

            //Se subscribe al NotifyPropertyChanged de la instancia si es que la instancia lo soporta:
            var Notifier = Instance as INotifyPropertyChanged;
            if (Notifier != null)
            {
                Notifier.PropertyChanged += (_, e) =>
                {
                    //Si la propiedad que cambio fue un CanExecute:
                    foreach (var M in Methods)
                    {
                        if (M.CanExecuteProperty == e.PropertyName)
                        {
                            //Obtiene el comando de este metodo:
                            var Command = inverseDic[M.Method];

                            //Informa que el can execute ha cambiado:
                            Command.RaiseCanExecuteChanged();
                        }

                    };
                };
            }
        }


        object IDynamicExtension.Get(string PropertyName)
        {
            return commands[PropertyName];
        }

        void IDynamicExtension.Set(string PropertyName, object Value)
        {
            throw new InvalidOperationException();
        }

        bool IDynamicExtension.CanRead(string PropertyName)
        {
            return true;
        }

        bool IDynamicExtension.CanWrite(string PropertyName)
        {
            return false;
        }

        Type IDynamicExtension.GetPropertyType(string PropertyName)
        {
            return typeof(ICommand);
        }

        IEnumerable<string> IDynamicExtension.MemberNames
        {
            get
            {
                return commands.Keys;
            }
        }
    }
}
