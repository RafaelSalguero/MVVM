using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Tonic.Excel.Printers;
using Kea;
namespace Tonic.Excel
{
    public class WorksheetData
    {
        public WorksheetData(string Title, IPrinter Printer)
        {
            this.Title = Title;
            this.Printer = Printer;
        }

        public string Title { get; set; }
        public IPrinter Printer { get; set; }
    }

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
        public static async Task<byte[]> SingleSheetExport(string Title, IPrinter Printer, Action<double> ReportProgress)
        {
            using (var p = new ExcelPackage())
            {
                p.Workbook.Properties.Title = Title;

                p.Workbook.Worksheets.Add(Title);
                var ws = p.Workbook.Worksheets[1];

                await Printer.Print(ws, 0, 0, ReportProgress);

                var ret = p.GetAsByteArray();

                return ret;
            }
        }
        /// <summary>
        /// Exporta un conjunto de IPrinters como un documento de excel de varias hojas
        /// </summary>
        public static async Task<byte[]> MultiSheetExport(string Title, IEnumerable<WorksheetData> Worksheets, Action<double> ReportProgress)
        {
            int Time = Worksheets.Sum(x => x.Printer.Time);
            var Prog = new Progress<double>(ReportProgress).Child(0, Time);

            double progressAcum = 0.0;
            using (var p = new ExcelPackage())
            {
                p.Workbook.Properties.Title = Title;
                foreach (var W in Worksheets)
                {
                    var ChildProg = Prog.Child(1);
                    Action<double> ChildProgress = x => ChildProg.Report(x * W.Printer.Time );

                  
                    var ws = p.Workbook.Worksheets.Add(W.Title);

                    await W.Printer.Print(ws, 0, 0, ChildProgress);
                   
                    progressAcum += W.Printer.Time;
                }

                var ret = p.GetAsByteArray();
                return ret;
            }
        }
    }
}
