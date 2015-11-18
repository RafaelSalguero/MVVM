using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Print a cell value table
    /// </summary>
    public class GridPrinter : IPrinter
    {
        /// <summary>
        /// Create a new cell printer
        /// </summary
        /// <param name="Value">The value of the cell</param>
        /// <param name="Format">The cell format</param>
        /// <param name="GreenBar">True to darken the background of alternating rows</param>
        /// <param name="FirstGreenBar">True to affect the first row with the green bar effect</param>
        public GridPrinter(string[,] Value, CellFormat Format, bool FirstGreenBar)
        {
            this.value = Value;
            this.format = Format;
            this.firstGreenBar = FirstGreenBar;
        }

        /// <summary>
        /// Create a new cell printer
        /// </summary
        /// <param name="Value">The data to convert to an excel report</param>
        public static GridPrinter Grid<T>(IEnumerable<T> Data, CellFormat Format)
        {
            return new GridPrinter(Tonic.Excel.Data.ToGrid(Data.Cast<object>(), Tonic.Excel.DataColumn.FromType(typeof(T))), Format, false);
        }

        /// <summary>
        /// Create a new cell printer
        /// </summary
        /// <param name="Value">The data to convert to an excel report</param>
        public static GridPrinter Grid<T>(IEnumerable<T> Data)
        {
            return Grid(Data, new CellFormat());
        }

        public static GridPrinter Cell(string Value, CellFormat Format)
        {
            return new GridPrinter(new[,] { { Value } }, Format, true);
        }


        public static GridPrinter Row(IEnumerable<string> Value, CellFormat Format)
        {
            var vs = Value.ToArray();
            var values = new string[1, vs.Length];
            for (int i = 0; i < vs.Length; i++)
                values[0, i] = vs[i];

            return new GridPrinter(values, Format, true);
        }

        public static GridPrinter Column(IEnumerable<string> Value, CellFormat Format, bool FirstGreenBar = false)
        {
            var vs = Value.ToArray();
            var values = new string[vs.Length, 1];
            for (int i = 0; i < vs.Length; i++)
                values[i, 0] = vs[i];

            return new GridPrinter(values, Format, FirstGreenBar);
        }

        readonly string[,] value;
        readonly CellFormat format;
        readonly bool firstGreenBar;



        public int Print(ExcelWorksheet ws, int startX, int startY)
        {
            bool gb = firstGreenBar;
            for (int y = startY; y < startY + value.GetLength(0); y++)
            {
                for (int x = startX; x < startX + value.GetLength(1); x++)
                {
                    var cell = ws.Cells[y + 1, x + 1];

                    cell.Value = value[y - startY, x - startX];
                    format.LoadStyle(cell.Style, format.GreenBar && gb);
                }
                gb = !gb;
            }
            return startY + value.GetLength(0);
        }
    }
}
