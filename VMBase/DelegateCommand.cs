using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tonic.MVVM
{
    /// <summary>
    /// Un comando que expone un delegado como accion. Considere usar la extension dinamica CommandsExtension en lugar de exponer propiedades como ICommands
    /// </summary>
    public class DelegateCommand : ICommand
    {

        /// <summary>
        /// Crea un nuevo DelegateCommand
        /// </summary>
        /// <param name="Action">La acción que se realizará al ejecutar el comando</param>
        /// <param name="CanExecute">Determina si el comando se puede ejecutar</param>
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

        /// <summary>
        /// Crea un nuevo DelegateCommand
        /// </summary>
        /// <param name="Action">La acción que se realizará al ejecutar el comando</param>
        /// <param name="CanExecute">Determina si el comando se puede ejecutar</param>
        public DelegateCommand(Action Action, Func<bool> CanExecute) : this(o => Action(), o => CanExecute()) { }

        TaskScheduler UiThread;

        /// <summary>
        /// Crea un nuevo DelegateCommand
        /// </summary>
        /// <param name="Action">La acción que se realizará al ejecutar el comando</param>
        public DelegateCommand(Action Action) : this(o => Action(), o => true) { }

        /// <summary>
        /// Crea un nuevo DelegateCommand
        /// </summary>
        /// <param name="Action">La acción que se realizará al ejecutar el comando</param>
        public DelegateCommand(Func<Task> Action) : this(o => Action(), o => true) { }

        /// <summary>
        /// Crea un nuevo DelegateCommand
        /// </summary>
        /// <param name="Action">La acción que se realizará al ejecutar el comando</param>
        public DelegateCommand(Action<object> Action) : this(Action, o => true) { }

        private readonly Action<object> action;

        /// <summary>
        /// Se dispara al llamar al metodo RaiseCanExecuteChanged
        /// </summary>
        public event EventHandler CanExecuteChanged;

        private readonly Func<object, bool> canExecute;

        /// <summary>
        /// Dispara el evento RaiseCanExecuteChanged en el hilo en el que el comando fue creado
        /// </summary>
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

        /// <summary>
        /// Ejecuta el comando siempre y cuando canExecute sea true
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (canExecute(parameter))
                action(parameter);
        }
    }

}
