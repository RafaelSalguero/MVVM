using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kea.GridData;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Drawing;

namespace Kea.PDF
{
    public class TablePrinter
    {
        public TablePrinter(IEnumerable<object> objects, IEnumerable<DataColumn> columns)
        {
            this.objects = objects.ToList();
            this.columns = columns.ToList();
            var valueObj = Data.ToGrid(objects, columns);
            this.value = Data.ToString(valueObj, columns.Select(x => x.Format).ToList());
        }
        readonly IReadOnlyList<object> objects;
        readonly IReadOnlyList<DataColumn> columns;
        readonly string[,] value;

        public static string ToString(object Value, string Format)
        {
            if (Value == null)
                return "";
            if (string.IsNullOrEmpty(Format))
                return Value.ToString();
            else
                return ((dynamic)Value).ToString(Format);
        }


     
        public static double[] GetTableWidths(string[] headers, string[,] data, Font Font)
        {
            var T = new TextMeasurement(Font);
            var result = new double[data.GetLength(1)];

            var margin = 0.5;
            for (int x = 0; x < headers.Length; x++)
            {
                var size = T.MeasureString(headers[x], UnitType.Centimeter);
                result[x] = Math.Max(result[x], size.Width + margin);
            }

            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    var size = T.MeasureString(data[y, x], UnitType.Centimeter);
                    result[x] = Math.Max(result[x], size.Width + margin);
                }
            }
            return result;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="Data">Colección de objetos de la que para cada objeto seran las filas y para cada propiedad publica las columnas</param>
        /// <param name="ExcludeColumns">Nombres de las propiedades de las columnas que se excluiran del reporte</param>
        public static TablePrinter Create<T>(IEnumerable<T> Data, IEnumerable<string> ExcludeColumns)
        {
            var Columns =
                DataColumn.FromData(Data, Prop => !ExcludeColumns.Contains(Prop));
            var ret = new TablePrinter(Data.Cast<object>(), Columns);
            return ret;
        }

        /// <summary>
        /// Crea un nuevo TablePrinter a partir de una colección de objetos, tomando la definición de las columnas automaticamente a partir de las propiedades del tipo T
        /// </summary>
        /// <param name="Data">Colección de objetos de la que para cada objeto seran las filas y para cada propiedad publica las columnas</param>
        public static TablePrinter Create<T>(IEnumerable<T> Data)
        {
            return Create(Data, new string[0]);
        }

        public Table Create()
        {
            var table = new Table();
            table.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;

            //Obtiene los anchos de la tabla:
            var widths = GetTableWidths(columns.Select(x => x.FriendlyName).ToArray(), value, new Font("Arial", Unit.FromPoint(11)));
            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].Width == null)
                    table.AddColumn(Unit.FromCentimeter(widths[i]));
                else
                    table.AddColumn(Unit.FromInch(columns[i].Width.Value / 96.0));
            }
            

            //cabecera:
            {
                var header = table.AddRow();

                //Agrega las celdas de la cabecera:
                header.Shading.Color = Colors.Gold;
                for (int i = 0; i < columns.Count; i++)
                {
                    header.Cells[i].AddParagraph(columns[i].FriendlyName);
                }
            }

            //Agrega los datos:
            for (int y = 0; y < value.GetLength(0); y++)
            {
                var row = table.AddRow();
                bool greenbar = y % 2 == 1;
                if (greenbar)
                    row.Shading.Color = new Color(220, 220, 220);

                for (int x = 0; x < value.GetLength(1); x++)
                {
                    var text = value[y, x];
                    row.Cells[x].AddParagraph(text);
                }
            }

            return table;
        }
    }
}
