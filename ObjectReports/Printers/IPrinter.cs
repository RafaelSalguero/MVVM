using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
namespace Tonic.Excel.Printers
{
    public interface IPrinter
    {
        /// <summary>
        /// Print the given data on the worksheet and returns startY + the number of printed rows
        /// </summary>
        int Print(ExcelWorksheet ws, int startX, int startY);
    }
}
