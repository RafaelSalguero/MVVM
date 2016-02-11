using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM;

namespace ViewBehavioursTest
{
    public enum TestEnum
    {
        Hola,

        [Description("Rafael Salguero")]
        Rafa,
        HolaRafa
    }

    public class Model
    {

    }

    public class ViewModel : ExposedViewModel<Model>
    {
        public ViewModel()
        {
            AddExtension(new Tonic.MVVM.Extensions.CommandsExtension(this));


            //Prueba los comandos y el cambio de CanExecute de manera asíncrona
            Task.Run(async () =>
           {
               while (true)
               {
                   try
                   {
                       HolaEnabled = !HolaEnabled;
                       await Task.Delay(1000);
                   }
                   catch (Exception ex)
                   {

                       throw;
                   }

               }
           });
        }

        bool close;
        public bool Close
        {
            get
            {
                return close;
            }
            set
            {
                close = value;
            }
       }

        public TestEnum Item { get; set; }
        public object Item2 { get; set; }
        public bool HolaEnabled { get; set; }
        public void Hola()
        {
        }
    }
}
