using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Prints a single value on a merged cell
    /// </summary>
    public class MergePrinter : IPrinter
    {
        public MergePrinter(string Value) : this(Value, 1, 1) { }
        public MergePrinter(string Value, int Width, int Height) : this(Value, Width, Height, new CellFormat())
        {

        }
        public MergePrinter(string Value, int Width, int Height, CellFormat Format)
        {
            this.Value = Value;
            this.width = Width;
            this.height = Height;
            this.format = Format;
        }

        readonly CellFormat format;
        readonly string Value;
        readonly int width, height;
        public int Print(ExcelWorksheet ws, int startX, int startY)
        {
          
            var cells = ws.Cells[startY + 1, startX + 1, startY + height , startX + width ];
            cells.Merge = true;
            cells.Value = Value;
            format.LoadStyle(cells.Style, format.GreenBar);

            return startY + height;
        }
    }
}
