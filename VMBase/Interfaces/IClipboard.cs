using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.MVVM.Interfaces
{
    /// <summary>
    /// Clipboard dependency
    /// </summary>
    public interface IClipboard
    {
        /// <summary>
        /// Gets text from the clipboard
        /// </summary>
        string GetText();
        /// <summary>
        /// Set text to the clipboard
        /// </summary>
        void SetText( string Text );
        /// <summary>
        /// Returns true if the clipboard have text
        /// </summary>
        /// <returns></returns>
        bool HasText();
    }
    /// <summary>
    /// IClipboard mock
    /// </summary>
    public class ClipboardMock : IClipboard
    {
         string IClipboard.GetText()
        {
            return "";
        }
         bool IClipboard.HasText()
        {
            return false;
        }
         void IClipboard.SetText(string Text)
        {
        }
    }
}
