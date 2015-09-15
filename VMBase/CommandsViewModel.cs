using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tonic.MVVM
{
    /// <summary>
    /// Expose public methods as commands
    /// </summary>
    public class CommandsViewModel : BaseViewModel
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

        public CommandsViewModel()
        {
            var Type = GetType();

            //Busca todos los metodos
            foreach (var M in Type.GetMethods())
            {
                var CommandName = M.Name + "Command";
                //Si el miembro command ya existe, lo ignora:
                if (Type.GetProperty(CommandName) != null)
                    continue;

                var P = M.GetParameters();
                switch (P.Length)
                {
                    case 0:
                        commands.Add(CommandName, new DelegateCommand(() => M.Invoke(this, new object[0])));
                        break;
                    case 1:
                        commands.Add(CommandName, new DelegateCommand((o) => M.Invoke(this, new object[] { o })));
                        break;
                }
            }
        }

        protected override object DynamicGet(string PropertyName)
        {
            DelegateCommand ret;
            var succeed = commands.TryGetValue(PropertyName, out ret);
            if (succeed)
                return ret;
            else
                throw new DynamicGetException();
        }
    }
}
