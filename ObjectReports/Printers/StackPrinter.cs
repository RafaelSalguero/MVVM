using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Prints other printers in vertical order
    /// </summary>
    public class StackPrinter : IPrinter 
    {
        public StackPrinter (IEnumerable <IPrinter > Printers)
        {
            this.printers = Printers;
        }
        private readonly IEnumerable<IPrinter> printers;

        public int Print(ExcelWorksheet ws, int startX, int startY)
        {
            int y = startY;
            foreach(var p in printers )
            {
                y = p.Print(ws, startX, y);
            }
            return y;
        }
    }

}
