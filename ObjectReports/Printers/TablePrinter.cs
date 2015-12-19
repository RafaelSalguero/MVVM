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
        public TablePrinter(IEnumerable<object> Objects, IEnumerable<DataColumn> Columns)
        {
            this.Objects = Objects.ToList();
            this.Columns = Columns.ToList();
            height = this.Objects.Count + 1;
            this.time = this.Objects.Count * this.Columns.Count;
            this.value = Data.ToGrid(Objects, Columns);
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter Create<T>(IEnumerable<T> Data)
        {
            return new TablePrinter(Data.Cast<object>(), DataColumn.FromData(Data));
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter Create<T>(IEnumerable<T> Data, bool FreezeHeader)
        {
            var ret = new TablePrinter(Data.Cast<object>(), DataColumn.FromData(Data));
            ret.FreezeHeader = FreezeHeader;
            return ret;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter Create<T>(IEnumerable<T> Data, bool FreezeHeader, Func<T, CellFormat> ConditionalFormat)
        {
            var ret = new TablePrinter(Data.Cast<object>(), DataColumn.FromData(Data));
            ret.FreezeHeader = FreezeHeader;
            ret.CellFormatOverride = o => ConditionalFormat((T)o);
            return ret;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="ExcludeColumns">Nombres de las propiedades de las columnas que se excluiran del reporte</param>
        public static TablePrinter Create<T>(IEnumerable<T> Data, bool FreezeHeader, Func<T, CellFormat> ConditionalFormat, IEnumerable<string> ExcludeColumns)
        {
            var Columns =
                DataColumn.FromData(Data, Prop => !ExcludeColumns.Contains (Prop ));


            var ret = new TablePrinter(Data.Cast<object>(), Columns);
            ret.FreezeHeader = FreezeHeader;
            ret.CellFormatOverride = o => ConditionalFormat((T)o);
            return ret;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        public static TablePrinter CreateHeaderless<T>(IEnumerable<T> Data)
        {
            var ret = new TablePrinter(Data.Cast<object>(), DataColumn.FromData(Data));
            ret.FreezeHeader = false;
            ret.PrintHeader = false;
            return ret;
        }

        int height;
        int IPrinter.Height
        {
            get
            {
                return height;
            }
        }
        int time;
        int IPrinter.Time => time;


        IReadOnlyList<object> Objects { get; set; }
        IReadOnlyList<DataColumn> Columns { get; set; }

        /// <summary>
        /// Formato de la primera fila que contiene los nombres de las columnas
        /// </summary>
        public CellFormat HeaderFormat { get; set; } = new CellFormat { Background = Colors.Gold, Bold = true };

        /// <summary>
        /// Formato condicional de las celdas que contienen los datos. Se tomara en cuenta si no es nulo
        /// </summary>
        public Func<object, CellFormat> CellFormatOverride { get; set; } = o => null;

        /// <summary>
        /// Formato de las celdas que contienen los datos
        /// </summary>
        public CellFormat DataFormat { get; set; } = new CellFormat { GreenBar = true };

        /// <summary>
        /// True para aplicar un autofit a las columnas impresas
        /// </summary>
        public bool AutoFit { get; set; } = true;

        /// <summary>
        /// True para congelar la cabecera
        /// </summary>
        public bool FreezeHeader { get; set; } = true;

        /// <summary>
        /// True para imprimir una cabezera
        /// </summary>
        public bool PrintHeader { get; set; } = true;

        readonly object[,] value;
        async Task PrintContent(ExcelWorksheet ws, int startX, int startY, Action<double> ReportProgress)
        {
            //Establece el formato para todas las celdas:
            var allCellAddress = new ExcelAddress(startY + 1, startX + 1, startY + value.GetLength(0), startX + value.GetLength(1));

            var adr = allCellAddress.Address;
            DataFormat.LoadStyle(ws.Cells[allCellAddress.Address].Style, false);



            //Establece los formatos numericos para las celdas que tienen:
            for (int i = 0; i < Columns.Count; i++)
            {
                var C = Columns[i];
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
                await Task.Run(() =>
                {
                    for (int x = startX; x < startX + value.GetLength(1); x++)
                    {
                        var cell = ws.Cells[y + 1, x + 1];
                        cell.Value = value[y - startY, x - startX];
                    }
                });

                var Format = CellFormatOverride(this.Objects[y - startY]);
                if (Format != null)
                {
                    var row = new ExcelAddress(y + 1, startX + 1, y + 1, startX + value.GetLength(1));
                    FormattedRows.Add(y);

                    var cond = ws.ConditionalFormatting.AddExpression(row);
                    cond.Style.Fill.BackgroundColor.Color = CellFormat.FromColor(Format.Background);
                    cond.Style.Font.Color.Color = CellFormat.FromColor(Format.Foreground);
                    cond.Formula = "TRUE";
                }

                if (y % 50 == 0)
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


        async Task IPrinter.Print(ExcelWorksheet ws, int StartX, int StartY, Action<double> ReportProgress)
        {
            if (FreezeHeader)
            {
                ws.View.FreezePanes(2 + StartY, StartX + 1);
            }

            var cols = Columns.ToArray();

            var GridData = Data.ToGrid(Objects, cols);
            //var Data = Kea.GridData.Data.ToGrid(Objects, Columns);
            if (PrintHeader)
            {
                IPrinter HeaderPrinter = GridPrinter.Row(Columns.Select(x => x.FriendlyName), HeaderFormat);
                await HeaderPrinter.Print(ws, StartX, StartY, x => { });
                StartY++;
            }

            await PrintContent(ws, StartX, StartY, x => ReportProgress(x * 2.0 / 3.0));

            //Fit columns:
            if (AutoFit)
            {
                for (int x = StartX; x < StartX + Columns.Count; x++)
                {
                    await Task.Run(() => ws.Column(x + 1).AutoFit());

                    var prog = (x - StartX) / (double)(Columns.Count);
                    ReportProgress((prog + 2.0) / 3.0);
                }
            }

        }
    }
}
