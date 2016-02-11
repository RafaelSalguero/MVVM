using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprime un unico valor en una celda combinada
    /// </summary>
    public class MergePrinter : IPrinter
    {
        /// <summary>
        /// Imprime un solo valor en una celda unica
        /// </summary>
        public MergePrinter(object Value) : this(Value, 1, 1) { }

        /// <summary>
        /// Imprime un solo valor en una celda combinada
        /// </summary>
        /// <param name="Value">El valor a imprimir</param>
        /// <param name="Width">El numero de celdas horizontales combinadas</param>
        /// <param name="Height">El número de celdas verticales combinadas</param>
        public MergePrinter(object Value, int Width, int Height) : this(Value, Width, Height, new CellFormat())
        {

        }

        /// <summary>
        /// Imprime un solo valor en una celda combinada
        /// </summary>
        /// <param name="Value">El valor a imprimir</param>
        /// <param name="Width">El numero de celdas horizontales combinadas</param>
        public MergePrinter(object Value, int Width) : this(Value, Width, 1, new CellFormat())
        {

        }

        /// <summary>
        /// Imprime un solo valor en una celda combinada
        /// </summary>
        /// <param name="Value">El valor a imprimir</param>
        /// <param name="Width">El numero de celdas horizontales combinadas</param>
        /// <param name="Height">El número de celdas verticales combinadas</param>
        /// <param name="Format">Formato de la celda</param>
        public MergePrinter(object Value, int Width, int Height, CellFormat Format)
        {
            this.Value = Value;
            this.width = Width;
            this.height = Height;
            this.format = Format;
        }

        /// <summary>
        /// Imprime un solo valor en una celda combinada
        /// </summary>
        /// <param name="Value">El valor a imprimir</param>
        /// <param name="Width">El numero de celdas horizontales combinadas</param>
        /// <param name="Height">El número de celdas verticales combinadas</param>
        /// <param name="xOffset">Desplazamiento horizontal</param>
        /// <param name="yOffset">Desplazamiento vertical</param>
        /// <param name="Format">Formato de la celda</param>
        public MergePrinter(object Value, int Width, int Height, int xOffset, int yOffset, CellFormat Format)
        {
            this.Value = Value;
            this.width = Width;
            this.height = Height;
            this.xOffset = xOffset;
            this.yOffset = yOffset;

            this.format = Format;
        }

        /// <summary>
        /// Imprime un solo valor en una celda combinada
        /// </summary>
        /// <param name="Value">El valor a imprimir</param>
        /// <param name="Width">El numero de celdas horizontales combinadas</param>
        /// <param name="Format">Formato de la celda</param>
        public MergePrinter(object Value, int Width, CellFormat Format) : this(Value, Width, 1, Format)
        {
        }

        readonly CellFormat format;
        readonly object Value;
        readonly int width, height;
        readonly int xOffset, yOffset;

        int IPrinter.Time
        {
            get
            {
                return 1;
            }
        }

        int IPrinter.Height
        {
            get
            {
                return height + yOffset;
            }
        }

        async Task IPrinter.Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress)
        {
            startX += xOffset;
            startY += yOffset;

            var cells = ws.Cells[startY + 1, startX + 1, startY + height, startX + width];
            cells.Merge = true;
            cells.Value = Value;
            format.LoadStyle(cells.Style, format.GreenBar);

            Progress(1);
        }
    }
}
