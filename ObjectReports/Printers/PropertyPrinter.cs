using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Imprime todas las propiedades publicas de un objeto que se puedan leer
    /// </summary>
    public class PropertyPrinter : IPrinter
    {
        public PropertyPrinter(object obj)
        {
            var datos = obj
                .GetType()
                .GetProperties()
                .Where(x => x.CanRead)
                .Select(x => new PropertyValue(x.Name, x.GetValue(obj)));

            this.table = TablePrinter.Create(datos);
        }


        readonly TablePrinter table;

        class PropertyValue
        {
            public PropertyValue(string name, object value)
            {
                this.Nombre = name;
                this.Valor = value;
            }

            public string Nombre { get; }
            public object Valor { get; }
        }

        public int Height => table.Height;

        public int Time => table.Time;

        public int Width => table.Width;

        public void Print(ExcelWorksheet ws, int startX, int startY, Action<double> Progress)
        {
            table.Print(ws, startX, startY, Progress);
        }
    }
}
