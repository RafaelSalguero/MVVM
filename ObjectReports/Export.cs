using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Tonic.Excel.Printers;

namespace Tonic.Excel
{
    public static class Export
    {
        public static byte[] SingleSheetExport(string Title, IPrinter Printer)
        {
            return SingleSheetExport(Title, ws => Printer.Print(ws, 0, 0));
        }
        /// <summary>
        /// Export a printer onto a byte array containing an excel xlsx file
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Printer"></param>
        /// <returns></returns>
        public static byte[] SingleSheetExport(string Title, Action<ExcelWorksheet> Printer)
        {
            using (var p = new ExcelPackage())
            {
                p.Workbook.Properties.Title = Title;

                p.Workbook.Worksheets.Add(Title);
                var ws = p.Workbook.Worksheets[1];

                Printer(ws);

                return p.GetAsByteArray();
            }
        }
    }
}
