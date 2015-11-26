using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Kea.GridData;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprime una fila especial con los nombres de las columnas seguida de una matriz de datos
    /// </summary>
    public class TablePrinter : IPrinter
    {
        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos y de una definicion de columnas
        /// </summary>
        /// <param name="Objects">Colección de objetos</param>
        /// <param name="Columns">Columnas a imprimir</param>
        public TablePrinter(IEnumerable<object> Objects, IEnumerable<DataColumn> Columns)
        {
            this.Objects = Objects;
            this.Columns = Columns;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter Create<T>(IEnumerable<T> Data)
        {
            return new TablePrinter(Data.Cast<object>(), DataColumn.FromData(Data));
        }




        IEnumerable<object> Objects { get; set; }
        IEnumerable<DataColumn> Columns { get; set; }

        /// <summary>
        /// Formato de la primera fila que contiene los nombres de las columnas
        /// </summary>
        public CellFormat HeaderFormat { get; set; } = new CellFormat { Background = Colors.Gold, Bold = true };

        /// <summary>
        /// Formato condicional de las celdas que contienen los datos. Se tomara en cuenta si no es nulo
        /// </summary>
        public Func<object, CellFormat> CellFormatOverride { get; set; } = null;

        /// <summary>
        /// Formato de las celdas que contienen los datos
        /// </summary>
        public CellFormat CellFormat { get; set; } = new CellFormat { GreenBar = true };

        /// <summary>
        /// True para aplicar un autofit a las columnas impresas
        /// </summary>
        public bool AutoFit { get; set; } = true;

        int IPrinter.Print(ExcelWorksheet ws, int StartX, int StartY)
        {
            Func<object, CellFormat> cf = CellFormatOverride ?? (o => CellFormat);
            var cols = Columns.ToArray();

            var ObjectData = Objects.ToArray();
            var Formats = ObjectData.Select(cf).ToArray();

            //var Data = Kea.GridData.Data.ToGrid(Objects, Columns);

            IPrinter HeaderPrinter = GridPrinter.Row(Columns.Select(x => x.FriendlyName), HeaderFormat);

            //Arma las filas de la matriz utilizando el formato especifico para cada celda
            var RowPrinters = ObjectData.Select((o, i) => GridPrinter.Row(Data.ToRow(o, cols), cf(o), i % 2 == 1));

            IPrinter ContentPrinter = new StackPrinter(RowPrinters);

            var ret = ContentPrinter.Print(ws, StartX, HeaderPrinter.Print(ws, StartX, StartY));

            //Fit columns:
            if (AutoFit)
            {
                for (int x = StartX; x < StartX + Columns.Count(); x++)
                {
                    ws.Column(x + 1).AutoFit();
                }
            }
            return ret;
        }
    }
}
