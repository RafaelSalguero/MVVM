using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Objeto que es capaz de imprimir en una hoja de excel
    /// </summary>
    public interface IPrinter
    {
        /// <summary>
        /// Imprime la informacion del objeto en una hoja de trabajo de excel, devuelve startY mas el numero de filas impresas
        /// </summary>
        int Print(ExcelWorksheet ws, int startX, int startY);
    }
}
