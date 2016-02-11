using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.MVVM;

namespace VMBase.Test
{
    [TestClass]
    public class AsyncICommandTest
    {
        public async Task Method()
        {
            await Task.Delay(1000);
            throw new Exception("hola");
        }


        [TestMethod]
        public void AsyncICommand()
        {
            var Command = new DelegateCommand(Method);

            Command.Execute(null);

            //Se espera a que se lance la excepcion
            Thread.Sleep(2000);
        }
    }
}
