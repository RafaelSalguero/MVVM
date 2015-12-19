using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Contiene toda la información necesaria para imprimir un IPrinter
    /// </summary>
    public struct PrinterInstruction
    {
        /// <summary>
        /// Crea un nuevo PrinterInstruction
        /// </summary>
        public PrinterInstruction(ExcelWorksheet ws, int startX, int startY)
        {
            this.ws = ws;
            this.startX = startX;
            this.startY = startY;
        }

        /// <summary>
        /// Hoja de excel
        /// </summary>
        public ExcelWorksheet ws { get; private set; }

        /// <summary>
        /// Posicion horizontal base 0
        /// </summary>
        public int startX { get; private set; }

        /// <summary>
        /// Posición vertical base 0
        /// </summary>
        public int startY { get; private set; }
    }

    /// <summary>
    /// Objeto que es capaz de imprimir en una hoja de excel
    /// </summary>
    public interface IPrinter
    {
        /// <summary>
        /// Obtiene el numero de filas que imprimira este objeto
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Obtiene un numero sin unidad que representa el tiempo aproximado que tardara en dibujarse este objeto 
        /// en relación con los demás
        /// </summary>
        int Time { get; }

        /// <summary>
        /// Imprime la informacion del objeto en una hoja de trabajo de excel, devuelve startY mas el numero de filas impresas
        /// </summary>
        /// <param name="Progress">Reporta el progreso de la impresion, valor de 0 a 1</param>
        Task Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress);
    }


    public static class PrinterExtensions
    {
        public static Task Print(this IPrinter Printer, PrinterInstruction I, Action<double> Progress)
        {
            return Printer.Print(I.ws, I.startX, I.startY, Progress);
        }
    }
}
