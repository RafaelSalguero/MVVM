using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Avanza filas vacias en la impresion
    /// </summary>
    public class SpacePrinter : IPrinter
    {
        /// <summary>
        /// Avanza la cantidad de filas especificada
        /// </summary>
        public SpacePrinter(int rows)
        {
            this.rows = rows;
        }

        public SpacePrinter() : this(1) { }

        readonly int rows;

        int IPrinter.Print(ExcelWorksheet ws, int startX, int startY)
        {
            return startY + rows;
        }
    }
}
