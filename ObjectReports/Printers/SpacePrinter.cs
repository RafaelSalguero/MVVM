using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Avanza filas y columnas vacias en la impresion
    /// </summary>
    public class SpacePrinter : IPrinter
    {
        /// <summary>
        /// Avanza la cantidad de filas especificada
        /// </summary>
        /// <param name="rows">Cantidad de filas que se van a avanzar</param>
        /// <param name="cols">Cantidad de columnas que se van a avanzar</param>
        public SpacePrinter(int rows, int cols)
        {
            this.Height = rows;
            this.Width = cols;
        }

        /// <summary>
        /// Avanza la cantidad de filas especificada y cero columnas
        /// </summary>
        /// <param name="rows">Cantidad de filas</param>
        public SpacePrinter(int rows) : this(rows, 0)
        {

        }

        /// <summary>
        /// Avanza una fila y cero columnas
        /// </summary>
        public SpacePrinter() : this(1) { }

        /// <summary>
        /// Filas avanzadas
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Columnas avanzadas
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Tiempo relativo
        /// </summary>
        public int Time
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// No realiza ninguna accion
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="Progress"></param>
        public void Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress)
        {
            Progress(1);
        }
    }
}
