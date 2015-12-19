using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Kea;
namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprime multiples objetos IPrinter sin avanzar verticalmente
    /// </summary>
    public class MultiPrinter : IPrinter
    {
        /// <summary>
        /// Crea un StackPrinter con multiples objetos IPrinter
        /// </summary>
        public MultiPrinter(IEnumerable<IPrinter> Printers)
        {
            this.printers = Printers.ToList();
            this.rows = Printers.Max(x => x.Height);
            this.time = Printers.Sum(x => x.Time);
        }
        private readonly IReadOnlyList<IPrinter> printers;
        int rows;

        int IPrinter.Height => rows;
        int time;
        int IPrinter.Time => time;

        async Task IPrinter.Print(ExcelWorksheet ws, int startX, int startY, Action<double> ReportProgress)
        {
            //Obtiene todas las instrucciones de impresion:
            var Instructions = new List<PrinterInstruction>();

            var Prog = new Progress<double>(ReportProgress).Child(time);

            foreach (var P in printers)
            {
                var ChildProg = Prog.Child(1);
                Action<double> ChildProgress = x => ChildProg.Report(x * P.Time);

                await P.Print(ws, startX, startY, ChildProgress);
            }

        }
    }
}
