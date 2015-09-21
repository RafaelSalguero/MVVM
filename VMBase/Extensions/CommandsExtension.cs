using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tonic.MVVM.Extensions
{
    /// <summary>
    /// Expose public methods as commands
    /// </summary>
    public class CommandsExtension : IDynamicExtension
    {
        class DelegateCommand : ICommand
        {
            public DelegateCommand(Action<object> Action)
            {
                this.action = Action;
            }
            public DelegateCommand(Action Action) : this(o => Action()) { }

            private readonly Action<object> action;
            public event EventHandler CanExecuteChanged;

            private bool canExecute = true;
            public bool CanExecute
            {
                get
                {
                    return canExecute;
                }
                set
                {
                    canExecute = value;
                    CanExecuteChanged?.Invoke(this, new EventArgs());
                }
            }

            bool ICommand.CanExecute(object parameter)
            {
                return CanExecute;
            }

            public void Execute(object parameter)
            {
                action(parameter);
            }
        }

        private Dictionary<string, DelegateCommand> commands = new Dictionary<string, DelegateCommand>();

        IEnumerable<string> IDynamicExtension.MemberNames
        {
            get
            {
                return commands.Keys;
            }
        }

        private static IEnumerable<MethodInfo> getCommandMethods(object instance, IEnumerable<Type> exclude)
        {
            var Type = instance.GetType();

            var CompleteExclude = exclude.Concat(new Type[] { typeof(BaseViewModel) });
            HashSet<MethodInfo> excluded = new HashSet<MethodInfo>(CompleteExclude.SelectMany(x => x.GetMethods()));

            return Type.GetMethods().Where(x =>
            {
                var r = !x.IsSpecialName && !excluded.Any(y => y.Name == x.Name && x.DeclaringType == y.DeclaringType);
                var p = x.GetParameters();

                return r && p.Length <= 1;
            });
        }

        /// <summary>
        /// Expose public 0 and 1 parameter methods as command properties, with the 'Command' postfix
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

        /// <summary>
        /// Expose the given methods as command properties with the 'Command' postfix
        /// </summary>
        /// <param name="Instance">The instance of the type that contains the methods</param>
        /// <param name="Methods">The given methods to expose</param>
        public CommandsExtension(object Instance, IEnumerable<MethodInfo> Methods)
        {
            var InstanceType = Instance.GetType();
            foreach (var M in Methods)
            {

                var CommandName = M.Name + "Command";
                //Si el miembro command ya existe, lo ignora:
                if (InstanceType.GetProperty(CommandName) != null)
                    continue;

                var P = M.GetParameters();
                switch (P.Length)
                {
                    case 0:
                        commands.Add(CommandName, new DelegateCommand(() => M.Invoke(Instance, new object[0])));
                        break;
                    case 1:
                        commands.Add(CommandName, new DelegateCommand((o) => M.Invoke(Instance, new object[] { o })));
                        break;
                    default:
                        throw new ArgumentException($"The command for the method '{M.Name}' can't be exposed because it has too many parameters ({P.Length})");
                }
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
    }
}
