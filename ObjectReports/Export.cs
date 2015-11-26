using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Tonic.Excel.Printers;

namespace Tonic.Excel
{
    /// <summary>
    /// Provee métodos para exportar objetos IPrinter a archivos de excel
    /// </summary>
    public static class Export
    {
        /// <summary>
        /// Exporta un objeto IPrinter a un arreglo de bytes que contiene un archivo xlsx de excel
        /// </summary>
        /// <param name="Title">Titulo del archivo</param>
        /// <param name="Printer">Printer</param>
        public static byte[] SingleSheetExport(string Title, IPrinter Printer)
        {
            return SingleSheetExport(Title, ws => Printer.Print(ws, 0, 0));
        }

        /// <summary>
        /// Exporta un objeto IPrinter a un arreglo de bytes que contiene un archivo xlsx de excel
        /// </summary>
        /// <param name="Title">Titulo del archivo</param>
        /// <param name="Printer">Printer</param>
        public static byte[] SingleSheetExport(string Title, Action<ExcelWorksheet> Printer)
        {
            var st = new Stopwatch();
            using (var p = new ExcelPackage())
            {
                st.Start();
                p.Workbook.Properties.Title = Title;

                p.Workbook.Worksheets.Add(Title);
                var ws = p.Workbook.Worksheets[1];

                Printer(ws);

                st.Stop();
                Console.WriteLine("Printer time: " + st.ElapsedMilliseconds);
                st.Reset();
                st.Start();

                var ret = p.GetAsByteArray();

                st.Stop();
                Console.WriteLine("GetAsByteArray time: " + st.ElapsedMilliseconds);


                return ret;
            }
        }
    }
}
