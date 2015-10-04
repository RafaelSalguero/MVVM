using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewBehaviours.TestValidation
{
    public class ViewModel : INotifyDataErrorInfo
    {
        private int value;
        public int Value
        {
            get; set;
        }

        public bool UserValid
        {
            get; set;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors
        {
            get
            {
                return Value == 2;
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(Value))
            {
                if (Value == 2)
                    yield return "Error";
            }
        }
    }
}
