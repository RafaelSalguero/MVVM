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
    /// Imprime multiples objetos IPrinter sin avanzar verticalmente ni horizontalmente
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
            this.cols = Printers.Max(x => x.Width);
            this.time = Printers.Sum(x => x.Time);
        }
        private readonly IReadOnlyList<IPrinter> printers;
        readonly int rows;
        readonly int cols;

        /// <summary>
        /// Altura maxima de los objetos;
        /// </summary>
        public int Height => rows;
        /// <summary>
        /// Ancho máximo de los objetos
        /// </summary>
        public int Width => cols;
        readonly int time;
        /// <summary>
        /// Tiempo relativo
        /// </summary>
        public int Time => time;

        /// <summary>
        /// Imprime los objetos
        /// </summary>
        /// <param name="ws">Worksheet</param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="ReportProgress"></param>
        public void Print(ExcelWorksheet ws, int startX, int startY, Action<double> ReportProgress)
        {
            //Obtiene todas las instrucciones de impresion:
            var Instructions = new List<PrinterInstruction>();

            var Prog = new Progress<double>(ReportProgress).Child(time);

            foreach (var P in printers)
            {
                var ChildProg = Prog.Child(1);
                Action<double> ChildProgress = x => ChildProg.Report(x * P.Time);

                P.Print(ws, startX, startY, ChildProgress);
            }

        }
    }
}
