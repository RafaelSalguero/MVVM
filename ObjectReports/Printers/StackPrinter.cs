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
    /// Imprime multiples objetos IPrintes acomodados verticalmente uno debajo del otro
    /// </summary>
    public class StackPrinter : IPrinter
    {
        /// <summary>
        /// Crea un StackPrinter con multiples objetos IPrinter
        /// </summary>
        public StackPrinter(IEnumerable<IPrinter> Printers)
        {
            this.printers = Printers.ToList();
            this.rows = Printers.Sum(x => x.Height);
            this.time = Printers.Sum(x => x.Time);
        }
        private readonly IReadOnlyList<IPrinter> printers;

        int rows;
        int IPrinter.Height
        {
            get
            {
                return rows;
            }
        }

        int time;
        int IPrinter.Time
        {
            get
            {
                return time;
            }
        }

        async Task IPrinter.Print(ExcelWorksheet ws, int startX, int startY, Action<double> ReportProgress)
        {
            //Obtiene todas las instrucciones de impresion:
            var Instructions = new List<PrinterInstruction>();
            int y = startY;

            var Prog = new Progress<double>(ReportProgress).Child(time);

            foreach (var P in printers)
            {
                var ChildProg = Prog.Child(1);
                Action<double> ChildProgress = x => ChildProg.Report(x * P.Time);

                await P.Print(ws, startX, y, ChildProgress);
                y += P.Height;
            }

            ReportProgress(1);
        }
    }

}
