using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OfficeOpenXml.Style;

namespace Tonic.Excel.Printers
{
    public class CellFormat
    {
        public Color Background { get; set; } = Colors.White;
        public Color Foreground { get; set; } = Colors.Black;

        public bool Bold { get; set; }
        public bool GreenBar { get; set; }

        private static System.Drawing.Color FromColor(Color C)
        {
            return System.Drawing.Color.FromArgb(C.A, C.R, C.G, C.B);
        }

        public void LoadStyle(ExcelStyle Style, bool Greenbar)
        {
            var Format = this;
            Color Fill = Greenbar ? Color.Add(Color.Multiply(Format.Background, 0.8F), Color.Multiply(Colors.Black, 0.2F)) : Format.Background;

            if (Background == Colors.Transparent)
            {
                Style.Fill.PatternType = ExcelFillStyle.None;
            }
            else
            {
                Style.Fill.PatternType = ExcelFillStyle.Solid;
                Style.Fill.BackgroundColor.SetColor(FromColor(Fill));
            }
            Style.Font.Bold = Format.Bold;
            Style.Font.Color.SetColor(FromColor(Format.Foreground));
        }
    }
}
