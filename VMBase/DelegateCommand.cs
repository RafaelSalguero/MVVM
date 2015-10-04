using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tonic.MVVM
{
    class DelegateCommand : ICommand
    {
        public DelegateCommand(Action<object> Action, Func<object, bool> CanExecute)
        {
            this.action = Action;
            this.canExecute = CanExecute;

            //En ambiente de pruebas unitarias, no se podra obtener el hilo de la interfaz:
            try
            {
                UiThread = TaskScheduler.FromCurrentSynchronizationContext();
            }
            catch (Exception)
            {

            }
        }
        public DelegateCommand(Action Action, Func<bool> CanExecute) : this(o => Action(), o => CanExecute()) { }

        TaskScheduler UiThread;

        public DelegateCommand(Action Action) : this(o => Action(), o => true) { }

        private readonly Action<object> action;
        public event EventHandler CanExecuteChanged;

        private readonly Func<object, bool> canExecute;
        public void RaiseCanExecuteChanged()
        {
            //Ejecuta el CanExecute en el hilo de la interfaz de usuario
            //a diferencia de los bindings a propiedades, el habilitado del comando no soporta llamadas entre hilos
            //por lo que se necesita envolver el disparo del evento para que esto sea transparente en la clase que implemente
            //las propiedades 'Enabled'
            if (UiThread != null)
            {
                Task.Factory.StartNew(() => CanExecuteChanged?.Invoke(this, new EventArgs()), System.Threading.CancellationToken.None, TaskCreationOptions.None, UiThread);
            }
            else
            {
                //Puede ser que el UiThread no exista, por ejemplo, en pruebas unitarias
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }


        bool ICommand.CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (canExecute(parameter))
                action(parameter);
        }
    }

}
