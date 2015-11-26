using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprime multiples objetos IPrintes acomodados verticalmente uno debajo del otro
    /// </summary>
    public class StackPrinter : IPrinter
    {
        /// <summary>
        /// Crea un StackPrinter con multiples objetos IPrinter
        /// </summary>
        public StackPrinter(IEnumerable<IPrinter> Printers)
        {
            this.printers = Printers;
        }
        private readonly IEnumerable<IPrinter> printers;

        int IPrinter.Print(ExcelWorksheet ws, int startX, int startY)
        {
            int y = startY;
            foreach (var p in printers)
            {
                y = p.Print(ws, startX, y);
            }
            return y;
        }
    }

}
