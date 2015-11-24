using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    public class TablePrinter : IPrinter
    {
        public TablePrinter(IEnumerable<object> Objects, IEnumerable<DataColumn> Columns)
        {
            this.Objects = Objects;
            this.Columns = Columns;
        }

        public static TablePrinter Create<T>(IEnumerable<T> Data)
        {
            return new TablePrinter(Data.Cast<object>(), DataColumn.FromType(typeof(T)));
        }

        public IEnumerable<object> Objects { get; private set; }
        public IEnumerable<DataColumn> Columns { get; private set; }

        public CellFormat HeaderFormat { get; set; } = new CellFormat { Background = Colors.Gold, Bold = true };
        public CellFormat CellFormat { get; set; } = new CellFormat { GreenBar = true };

        public int Print(ExcelWorksheet ws, int StartX, int StartY)
        {
            var Data = Tonic.Excel.Data.ToGrid(Objects, Columns);
            var HeaderPrinter = GridPrinter.Row(Columns.Select(x => x.FriendlyName), HeaderFormat);
            var ContentPrinter = new GridPrinter(Data, CellFormat, false);

            var ret = ContentPrinter.Print(ws, StartX, HeaderPrinter.Print(ws, StartX, StartY));
            //Fit columns:

            for (int x = StartX; x < StartX + Columns.Count(); x++)
            {
                ws.Column(x + 1).AutoFit();
            }
            return ret;
        }
    }
}
