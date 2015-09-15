using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewLocatorTest
{
    public class TestModel : IDataErrorInfo
    {
        public string Title
        {
            get; set;
        } = "Hola rafa";

        public string Subtitle
        {
            get; set;
        } = "Como";

        private string message = "Andas";
        public string Message
        {

            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                return null;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Message):
                        if (Message == "holis")
                            return "El mensaje no puede ser holis";
                        break;
                }
                return null;
            }
        }
    }
}
