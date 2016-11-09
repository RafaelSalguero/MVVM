using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OfficeOpenXml.Style;

namespace Tonic.Excel.Printers
{
    /// <summary>
    /// Especifica el formato de una celda
    /// </summary>
    public class CellFormat
    {
        /// <summary>
        /// Crea un nuevo formato de celda
        /// </summary>
        /// <param name="foreground">Color de fuente</param>
        /// <param name="background">Color de fondo</param>
        /// <param name="bold">negritas</param>
        /// <param name="greenBar">Efecto green bar</param>
        /// <param name="numberFormat">Formato de celda</param>
        public CellFormat(
            Color foreground,
            Color background,
            bool bold,
            bool greenBar,
            string numberFormat
            )
        {
            this.Foreground = foreground;
            this.Background = background;
            this.Bold = bold;
            this.GreenBar = greenBar;
            this.NumberFormat = numberFormat;
        }

        /// <summary>
        /// CellFormat defalt
        /// </summary>
        public static readonly CellFormat Default = new CellFormat(Colors.Black, Colors.Transparent, false, false, null);

        /// <summary>
        /// CellFormat con green bar
        /// </summary>
        public static readonly CellFormat Data = new CellFormat(Colors.Black, Colors.Transparent, false, true, null);

        /// <summary>
        /// Cabecera
        /// </summary>
        public static readonly CellFormat Header = new CellFormat(Colors.Black, Colors.Gold, true, true, null);

        /// <summary>
        /// Color de fondo
        /// </summary>
        public Color Background { get; private set; } = Colors.Transparent;

        /// <summary>
        /// Color de fuente
        /// </summary>
        public Color Foreground { get; private set; } = Colors.Black;

        /// <summary>
        /// Negritas
        /// </summary>
        public bool Bold { get; private set; }

        /// <summary>
        /// Oscurecer la celdas impares
        /// </summary>
        public bool GreenBar { get; private set; }

        /// <summary>
        /// Formato de la celda
        /// </summary>
        public string NumberFormat { get; private set; }

        internal static System.Drawing.Color FromColor(Color C)
        {
            return System.Drawing.Color.FromArgb(C.A, C.R, C.G, C.B);
        }

        internal static Color Darken(Color C)
        {
            return Color.Add(Color.Multiply(C, 0.8F), Color.Multiply(Colors.Black, 0.2F));
        }

        /// <summary>
        /// Llena un objeto ExcelStyle con los parametros del formato de la celda
        /// </summary>
        /// <param name="Style">Objeto a llenar</param>
        /// <param name="Greenbar">False para ignorar el parametro greenbar</param>
        public void LoadStyle(ExcelStyle Style, bool Greenbar)
        {
            var Format = this;
            bool force = false;
            Color Fill = Greenbar ? Color.Add(Color.Multiply(Format.Background, 0.8F), Color.Multiply(Colors.Black, 0.2F)) : Format.Background;

            if (Background == Colors.Transparent)
            {
                if (!force)
                    Style.Fill.PatternType = ExcelFillStyle.None;
            }
            else
            {
                Style.Fill.PatternType = ExcelFillStyle.Solid;
                Style.Fill.BackgroundColor.SetColor(FromColor(Fill));
            }
            if (force || Format.Bold)
                Style.Font.Bold = Format.Bold;

            if (force || Format.Foreground != Colors.Black)
                Style.Font.Color.SetColor(FromColor(Format.Foreground));

            if (!string.IsNullOrEmpty(NumberFormat))
                Style.Numberformat.Format = NumberFormat;
        }
    }
}
