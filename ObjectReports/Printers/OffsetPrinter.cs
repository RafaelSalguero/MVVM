using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprime un printer aplicando un offset en la posición y el tamaño
    /// </summary>
    public class OffsetPrinter : IPrinter
    {
        public OffsetPrinter(IPrinter printer, int xOffset = 0, int yOffset = 0, int widthOffset = 0, int heightOffset = 0)
        {
            this.x = xOffset;
            this.y = yOffset;
            this.width = widthOffset;
            this.height = heightOffset;
        }

        readonly IPrinter printer;
        readonly int x, y, width, height;

        public int Height => printer.Height + height;

        public int Width => printer.Width + width;

        public int Time => printer.Time;

        public void Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress)
        {
              printer.Print(ws, startX + x, startY + y, Progress);
        }
    }
}
