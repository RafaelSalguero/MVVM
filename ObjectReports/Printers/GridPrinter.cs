using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Kea.GridData;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprima una tabla de valores
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
        public GridPrinter(object[,] Value, CellFormat Format, bool FirstGreenBar)
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
            return new GridPrinter(Kea.GridData.Data.ToGrid(Data.Cast<object>(), DataColumn.FromType(typeof(T))), Format, false);
        }

        /// <summary>
        /// Create a new cell printer
        /// </summary
        /// <param name="Value">The data to convert to an excel report</param>
        public static GridPrinter Grid<T>(IEnumerable<T> Data)
        {
            return Grid(Data, new CellFormat());
        }

        /// <summary>
        /// Crea un GridPrinter para una sola celda
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        public static GridPrinter Cell(object Value, CellFormat Format)
        {
            return new GridPrinter(new[,] { { Value } }, Format, true);
        }


        /// <summary>
        /// Crea un grid printer para una fila
        /// </summary>
        /// <param name="Value">Colección de valores a imprimir</param>
        /// <param name="Format">Formato de la celda</param>
        public static GridPrinter Row(IEnumerable<object> Value, CellFormat Format)
        {
            return Row(Value, Format, true);
        }


        /// <summary>
        /// Crea un grid printer para una fila
        /// </summary>
        /// <param name="Value">Colección de valores a imprimir</param>
        /// <param name="GreenBar">True para considerar el efecto green bar en esta fila</param>
        /// <param name="Format">Formato de la celda</param>
        public static GridPrinter Row(IEnumerable<object> Value, CellFormat Format, bool GreenBar)
        {
            var vs = Value.ToArray();
            var values = new object[1, vs.Length];
            for (int i = 0; i < vs.Length; i++)
                values[0, i] = vs[i];

            return new GridPrinter(values, Format, GreenBar);
        }


        /// <summary>
        /// Crea un grid printer para una columna
        /// </summary>
        /// <param name="Value">Colección de valores a imprimir</param>
        /// <param name="Format">Formato de las celdas</param>
        /// <param name="FirstGreenBar">True para establecer que el primer renglon sera considerado como impar</param>
        public static GridPrinter Column(IEnumerable<object> Value, CellFormat Format, bool FirstGreenBar = false)
        {
            var vs = Value.ToArray();
            var values = new object[vs.Length, 1];
            for (int i = 0; i < vs.Length; i++)
                values[i, 0] = vs[i];

            return new GridPrinter(values, Format, FirstGreenBar);
        }

        readonly object[,] value;
        readonly CellFormat format;
        readonly bool firstGreenBar;


        int IPrinter.Height
        {
            get
            {
                return value.GetLength(0);
            }
        }

        int IPrinter.Time
        {
            get
            {
                return value.Length;
            }
        }

        async Task IPrinter.Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress)
        {
            bool gb = firstGreenBar;

            //Establece el estilo de todas las celdas:
            format.LoadStyle(ws.Cells[startY + 1, startX + 1, startY + value.GetLength(0), startX + value.GetLength(1)].Style, false);

            for (int y = startY; y < startY + value.GetLength(0); y++)
            {
                await Task.Run(() =>
              {
                  for (int x = startX; x < startX + value.GetLength(1); x++)
                  {
                      var cell = ws.Cells[y + 1, x + 1];
                      cell.Value = value[y - startY, x - startX];
                  }
              });

                if (y % 50 == 0)
                    Progress((y - startY) / (double)value.GetLength(0));

                gb = !gb;
            }
            Progress(1);
        }
    }
}
