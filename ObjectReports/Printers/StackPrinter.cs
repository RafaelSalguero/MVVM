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
            this.cols = Printers.Max(x => x.Width);
            this.time = Printers.Sum(x => x.Time);
        }
        private readonly IReadOnlyList<IPrinter> printers;

        readonly int rows;
        readonly int cols;
        /// <summary>
        /// Suma de las alturas de los objetos
        /// </summary>
        public int Height => rows;
        /// <summary>
        /// Ancho máximo de los objetos
        /// </summary>
        public int Width => cols;
        int time;
        /// <summary>
        /// Tiempo relativo de impresión
        /// </summary>
        public int Time
        {
            get
            {
                return time;
            }
        }

        /// <summary>
        /// Realiza la impresión
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="ReportProgress"></param>
        public void Print(ExcelWorksheet ws, int startX, int startY, Action<double> ReportProgress)
        {
            //Obtiene todas las instrucciones de impresion:
            int y = startY;

            var Prog = new Progress<double>(ReportProgress).Child(time);

            foreach (var P in printers)
            {
                var ChildProg = Prog.Child(1);
                Action<double> ChildProgress = x => ChildProg.Report(x * P.Time);

                P.Print(ws, startX, y, ChildProgress);
                y += P.Height;
            }

            ReportProgress(1);
        }
    }

}
