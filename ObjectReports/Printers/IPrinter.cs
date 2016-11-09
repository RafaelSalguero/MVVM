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
    /// Objeto que es capaz de imprimir en una hoja de excel. Todos los objetos IPrinter son inmutables
    /// </summary>
    public interface IPrinter
    {
        /// <summary>
        /// Obtiene el numero de filas que imprimira este objeto
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Obtiene el numero de columnas que ocupa este objeto al ser impreso
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Obtiene un numero sin unidad que representa el tiempo aproximado que tardara en dibujarse este objeto 
        /// en relación con los demás
        /// </summary>
        int Time { get; }

        /// <summary>
        /// Imprime la informacion del objeto en una hoja de trabajo de excel, devuelve startY mas el numero de filas impresas
        /// </summary>
        /// <param name="Progress">Reporta el progreso de la impresion, valor de 0 a 1</param>
        /// <param name="startX">Inicio de la impresión horizontal, comienza en 0</param>
        /// <param name="startY">Inicio de la impresión vertical, comieza en 0</param>
        /// <param name="ws">Work sheet en el cual se va a imprimir</param>
        void Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress);
    }


    /// <summary>
    /// Metodos de ayuda para imprimir
    /// </summary>
    public static class PrinterExtensions
    {
        /// <summary>
        /// Imprime la informacion del objeto en una hoja de trabajo de excel, devuelve startY mas el numero de filas impresas
        /// </summary>
        /// <param name="Printer">Objeto que se va a imprimir</param>
        /// <param name="I">Información de la impresión</param>
        /// <param name="Progress">Reporte el progreso de la impresión. El valor va de 0 a 1</param>
        /// <returns></returns>
        public static void Print(this IPrinter Printer, PrinterInstruction I, Action<double> Progress)
        {
              Printer.Print(I.ws, I.startX, I.startY, Progress);
        }
    }
}
