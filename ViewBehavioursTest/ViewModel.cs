using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM;

namespace ViewBehavioursTest
{
    public class ViewModel : CommitViewModel
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

        public bool HolaEnabled { get; set; }
        public void Hola()
        {
        }
    }
}
