using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="autoFit">Establecer automaticamente el tamaño de las columnas</param>
        /// <param name="freezeHeaders">Congelar la cabecera de la tabla</param>
        /// <param name="printHeaders">Imprimir la cabecera</param>
        /// <param name="headerFormat">Formato de la cabecera</param>
        /// <param name="dataFormat">Formato de los datos</param>
        /// <param name="cellFormatOverride">Función de formate condicional, 
        /// si esta función devuelve un valor diferente de nulo se establece ese como 
        /// el formato de la celda. Si la función es null sera ignorada</param>
        public TablePrinter(
            IEnumerable<object> Objects,
            IEnumerable<DataColumn> Columns,
            bool freezeHeaders,
            bool autoFit,
            bool printHeaders,
            CellFormat headerFormat,
            CellFormat dataFormat,
            Func<object, CellFormat> cellFormatOverride
            )
        {
            this.FreezeHeader = freezeHeaders;
            this.AutoFit = autoFit;
            this.PrintHeader = printHeaders;
            this.DataFormat = dataFormat;
            this.HeaderFormat = headerFormat;
            this.CellFormatOverride = cellFormatOverride;

            this.objects = Objects.ToList();
            this.columns = Columns.ToList();
            this.time = this.objects.Count * this.columns.Count;
            this.value = Data.ToGrid(Objects, Columns);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter Create<T>(IEnumerable<T> data)
        {
            return Create(data, true);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="freezeHeader">True para congelar la cabecera de la tabla</param>
        /// <param name="data">Datos de la tabla</param>
        public static TablePrinter Create<T>(IEnumerable<T> data, bool freezeHeader)
        {
            return Create(data, freezeHeader, null);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter Create<T>(IEnumerable<T> data, bool freezeHeader, Func<T, CellFormat> conditionalFormat)
        {
            return Create(data, freezeHeader, conditionalFormat, new string[0]);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="excludeColumns">Nombres de las propiedades de las columnas que se excluiran del reporte</param>
        /// <param name="freezeHeader">True para congelar la cabecera de la tabla</param>
        /// <param name="cellFormatOverride">Formato condicional o null para no establecer ninguno</param>
        /// <param name="data">Datos de latabla</param>
        public static TablePrinter Create<T>(IEnumerable<T> data, bool freezeHeader, Func<T, CellFormat> cellFormatOverride, IEnumerable<string> excludeColumns)
        {
            return Create(data, freezeHeader, true, true, cellFormatOverride, excludeColumns);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="excludeColumns">Nombres de las propiedades de las columnas que se excluiran del reporte</param>
        /// <param name="freezeHeader">True para congelar la cabecera de la tabla</param>
        /// <param name="cellFormatOverride">Formato condicional o null para no establecer ninguno</param>
        ///  /// <param name="autoFit">True para establecer automaticamente el ancho de las columnas</param>
        /// <param name="printHeader">True para imprimir la cabecera de la tabla</param>
        /// <param name="data">Datos de latabla</param>
        public static TablePrinter Create<T>(IEnumerable<T> data, bool freezeHeader, bool autoFit, bool printHeader, Func<T, CellFormat> cellFormatOverride, IEnumerable<string> excludeColumns)
        {
            return Create(data, freezeHeader, autoFit, printHeader, CellFormat.Header, CellFormat.Data, cellFormatOverride, excludeColumns);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="excludeColumns">Nombres de las propiedades de las columnas que se excluiran del reporte</param>
        /// <param name="freezeHeader">True para congelar la cabecera de la tabla</param>
        /// <param name="cellFormatOverride">Formato condicional o null para no establecer ninguno</param>
        /// <param name="autoFit">True para establecer automaticamente el ancho de las columnas</param>
        /// <param name="printHeader">True para imprimir la cabecera de la tabla</param>
        /// <param name="data">Datos de latabla</param>
        /// <param name="headerFormat">Formato de la cabecera</param>
        /// <param name="dataFormat">Formato de los datos de la tabla</param>
        public static TablePrinter Create<T>(IEnumerable<T> data,
            bool freezeHeader,
            bool autoFit,
            bool printHeader,
            CellFormat headerFormat,
            CellFormat dataFormat,
            Func<T, CellFormat> cellFormatOverride,
            IEnumerable<string> excludeColumns)
        {
            var Columns =
                DataColumn.FromData(data, Prop => !excludeColumns.Contains(Prop));

            if (cellFormatOverride == null)
                cellFormatOverride = o => null;

            var ret = new TablePrinter(data.Cast<object>(),
                Columns,
                freezeHeader,
                autoFit,
                printHeader,
                headerFormat,
                dataFormat,
                o => cellFormatOverride((T)o));
            return ret;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter CreateHeaderless<T>(IEnumerable<T> data)
        {
            return Create(data, true, true, false, null, new string[0]);
        }

        /// <summary>
        /// Altura de la tabla
        /// </summary>
        public int Height => this.objects.Count + (PrintHeader ? 1 : 0);
        /// <summary>
        /// Cantidad de columnas
        /// </summary>
        public int Width => this.columns.Count;

        readonly int time;
        /// <summary>
        /// Tiempo relativo
        /// </summary>
        public int Time => time;


        readonly IReadOnlyList<object> objects;
        readonly IReadOnlyList<DataColumn> columns;

        /// <summary>
        /// Formato de la primera fila que contiene los nombres de las columnas
        /// </summary>
        public readonly CellFormat HeaderFormat;

        /// <summary>
        /// Formato condicional de las celdas que contienen los datos. Se tomara en cuenta si no es nulo
        /// </summary>
        public readonly Func<object, CellFormat> CellFormatOverride;

        /// <summary>
        /// Formato de las celdas que contienen los datos
        /// </summary>
        public readonly CellFormat DataFormat;

        /// <summary>
        /// True para aplicar un autofit a las columnas impresas
        /// </summary>
        public readonly bool AutoFit;

        /// <summary>
        /// True para congelar la cabecera
        /// </summary>
        public readonly bool FreezeHeader;

        /// <summary>
        /// True para imprimir una cabezera
        /// </summary>
        public readonly bool PrintHeader;

        /// <summary>
        /// Valores de la tabla
        /// </summary>
        readonly object[,] value;

        /// <summary>
        /// Imprime el contenido de la tabla
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="ReportProgress"></param>
        void PrintContent(ExcelWorksheet ws, int startX, int startY, Action<double> ReportProgress)
        {
            //Establece el formato para todas las celdas:
            var allCellAddress = new ExcelAddress(startY + 1, startX + 1, startY + value.GetLength(0), startX + value.GetLength(1));

            var adr = allCellAddress.Address;
            DataFormat.LoadStyle(ws.Cells[allCellAddress.Address].Style, false);

            //Establece los formatos numericos para las celdas que tienen:
            for (int i = 0; i < columns.Count; i++)
            {
                var C = columns[i];
                if (C.Format != null)
                {
                    var Cells = ws.Cells[startY + 1, startX + 1 + i, startY + value.GetLength(0), startX + 1 + i];
                    Cells.Style.Numberformat.Format = C.Format;
                }
            }

            //Establece los valores de las celdas:
            //Guarda las filas formateadas para poder agregar excepciones al formateo condicional
            var FormattedRows = new List<int>();
            for (int y = startY; y < startY + value.GetLength(0); y++)
            {
                for (int x = startX; x < startX + value.GetLength(1); x++)
                {
                    var cell = ws.Cells[y + 1, x + 1];
                    cell.Value = value[y - startY, x - startX];
                }

                var Format = CellFormatOverride(this.objects[y - startY]);
                if (Format != null)
                {
                    var row = new ExcelAddress(y + 1, startX + 1, y + 1, startX + value.GetLength(1));
                    FormattedRows.Add(y);

                    var cond = ws.ConditionalFormatting.AddExpression(row);
                    cond.Style.Fill.BackgroundColor.Color = CellFormat.FromColor(Format.Background);
                    cond.Style.Font.Color.Color = CellFormat.FromColor(Format.Foreground);
                    cond.Formula = "TRUE";
                }

                if (y % 200 == 0)
                    ReportProgress((y - startY) / (double)value.GetLength(0));
            }

            if (DataFormat.GreenBar)
            {
                //Establece un formato condicional para el greenbar:
                var cond = ws.ConditionalFormatting.AddExpression(allCellAddress);
                cond.Style.Fill.BackgroundColor.Color = CellFormat.FromColor(CellFormat.Darken(DataFormat.Background));
                cond.Formula = $"MOD(ROW()+{startY % 2},2)=0";
            }

        }


        /// <summary>
        /// Imprime la tabla
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        /// <param name="ReportProgress"></param>
        public void Print(ExcelWorksheet ws, int StartX, int StartY, Action<double> ReportProgress)
        {
            if (FreezeHeader)
            {
                ws.View.FreezePanes(2 + StartY, StartX + 1);
            }

            var cols = columns.ToArray();

            var GridData = Data.ToGrid(objects, cols);
            //var Data = Kea.GridData.Data.ToGrid(Objects, Columns);
            if (PrintHeader)
            {
                IPrinter HeaderPrinter = GridPrinter.Row(columns.Select(x => x.FriendlyName), HeaderFormat);
                HeaderPrinter.Print(ws, StartX, StartY, x => { });
                StartY++;
            }

            PrintContent(ws, StartX, StartY, x => ReportProgress(x * 1 / 2.0));

            //Fit columns:
            if (AutoFit)
            {
                for (int x = StartX; x < StartX + columns.Count; x++)
                {
                    ws.Column(x + 1).AutoFit();

                    var prog = (x - StartX) / (double)(columns.Count);

                    if (x % 5 == 0)
                        ReportProgress((prog + 1.0) / 2.0);
                }
            }

        }
    }
}
