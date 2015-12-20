using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tonic.MVVM.Interfaces;

namespace Tonic.MVVM.Dialogs
{
    /// <summary>
    /// View model for the exception view
    /// </summary>
    public class ExceptionViewModel : Tonic.MVVM.CommitViewModel
    {
        public ExceptionViewModel(Exception Ex, IClipboard Clipboard, IAssembly Assembly)
        {
            this.Clipboard = Clipboard;
            this.Assembly = Assembly;
            this.Item = new SingleExceptionViewModel(Ex);
        }
        readonly IClipboard Clipboard;
        readonly IAssembly Assembly;
        /// <summary>
        /// Copy item Text to the clipboard
        /// </summary>
        public ICommand CopyCommand => new DelegateCommand(() => Clipboard.SetText(Text));

        /// <summary>
        /// Exception
        /// </summary>
        public SingleExceptionViewModel Item { get; private set; }

        public string AssemblyName => Assembly.Name;
        public string AssemblyVersion => Assembly.Version;
        public DateTime  AssemblyDate => Assembly.Date;
        public string Text
        {
            get
            {
                StringBuilder B = new StringBuilder();
                B.AppendLine("Assembly : " + AssemblyName);
                B.AppendLine("Version  : " + AssemblyVersion);
                B.AppendLine("_________");
                B.AppendLine("Structure:");

                GetStructure(B, Item, 0, x => x.Append(""));

                B.AppendLine("Data:");

                B.AppendLine(Item.Text);
                return B.ToString();
            }
        }

        public static void GetStructure(StringBuilder B, SingleExceptionViewModel Item, int deep, Action<StringBuilder> AppendLine)
        {
            B.AppendLine();
            AppendLine(B);

            if (deep > 0)
                B.Append("");
            else
                B.Append("");

            B.Append(Item.Type);
            for (int i = 0; i < Item.InnerExceptions.Count; i++)
            {
                Action<StringBuilder> Ap;
                if (i == Item.InnerExceptions.Count - 1   )
                    Ap = x =>
                    {
                        AppendLine(x);
                        x.Append("  ");
                    };
                else
                    Ap = x =>
                    {
                        AppendLine(x);
                        x.Append("  ");
                    };

                GetStructure(B, Item.InnerExceptions[i], deep + 1, Ap);
            }
        }
    }

    /// <summary>
    /// View model for a single exception
    /// </summary>
    public class SingleExceptionViewModel
    {
        /// <summary>
        /// Create a new exception viewmodel
        /// </summary>
        public SingleExceptionViewModel(string Type, string Message, string StackTrace, IEnumerable<SingleExceptionViewModel> InnerExceptions)
        {
            this.Type = Type;
            this.Message = Message;
            this.StackTrace = StackTrace;
            this.InnerExceptions = InnerExceptions.ToList();
        }

        /// <summary>
        /// Create a new exception view model from a given exception
        /// </summary>
        public SingleExceptionViewModel(Exception Ex)
        {
            this.Type = Ex.GetType().ToString();
            this.Message = Ex.Message;
            this.StackTrace = Ex.StackTrace;
            this.InnerExceptions = GetInners(Ex).Select(x => new SingleExceptionViewModel(x)).ToList();
        }

        static IEnumerable<Exception> GetInners(Exception Ex)
        {
            if (Ex is AggregateException)
            {
                return ((AggregateException)Ex).InnerExceptions;
            }
            else if (Ex.InnerException != null)
                return new[] { Ex.InnerException };
            else
                return new Exception[0];

        }

        /// <summary>
        /// Exception type
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Exception message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Exception stack trace
        /// </summary>
        public string StackTrace { get; private set; }

        /// <summary>
        /// Inner exceptions
        /// </summary>
        public IReadOnlyList<SingleExceptionViewModel> InnerExceptions { get; private set; }

        /// <summary>
        /// Convert this object to an string representation
        /// </summary>
        /// <returns></returns>
        public string Text
        {
            get
            {
                StringBuilder B = new StringBuilder();

                B.AppendLine("**");
                B.AppendLine("Type       :" + Type);
                B.AppendLine("Message    :" + Message);
                B.AppendLine("StackTrace :" + StackTrace);
                B.AppendLine("___________");
                foreach (var I in InnerExceptions)
                {
                    B.AppendLine(I.Text);
                }
                return B.ToString();
            }
        }
    }
}
