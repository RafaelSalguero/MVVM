using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tonic.MVVM.Interfaces;

namespace Tonic.MVVM
{
    /// <summary>
    /// Windows clipboard
    /// </summary>
    public class Clipboard : IClipboard
    {
        public Clipboard ()
        {

        }

        public string GetText()
        {
            return System.Windows.Clipboard.GetText();
        }

        public bool HasText()
        {
            return System.Windows.Clipboard.ContainsText();
        }

        public void SetText(string Text)
        {
            System.Windows.Clipboard.SetText(Text);
        }
    }
}
