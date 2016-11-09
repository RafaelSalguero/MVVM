using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;

namespace Kea.PDF
{
    /// <summary>
    /// Provides functionality to measure the width of text during document design time.
    /// </summary>
    public sealed class TextMeasurement
    {
        /// <summary>
        /// Initializes a new instance of the TextMeasurement class with the specified font.
        /// </summary>
        public TextMeasurement(MigraDoc.DocumentObjectModel.Font font)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            this.font = font;
        }

        /// <summary>
        /// Returns the size of the bounding box of the specified text.
        /// </summary>
        public System.Drawing.SizeF MeasureString(string text, UnitType unitType)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (!Enum.IsDefined(typeof(UnitType), unitType))
                throw new ArgumentException();

            System.Drawing.Graphics graphics = Realize();

            SizeF size = graphics.MeasureString(text, this.gdiFont, new PointF(0, 0), System.Drawing.StringFormat.GenericTypographic);
            switch (unitType)
            {
                case UnitType.Point:
                    break;

                case UnitType.Centimeter:
                    size.Width = (float)(size.Width * 2.54 / 72);
                    size.Height = (float)(size.Height * 2.54 / 72);
                    break;

                case UnitType.Inch:
                    size.Width = size.Width / 72;
                    size.Height = size.Height / 72;
                    break;

                case UnitType.Millimeter:
                    size.Width = (float)(size.Width * 25.4 / 72);
                    size.Height = (float)(size.Height * 25.4 / 72);
                    break;

                case UnitType.Pica:
                    size.Width = size.Width / 12;
                    size.Height = size.Height / 12;
                    break;

                default:
                    break;
            }
            return size;
        }

        /// <summary>
        /// Returns the size of the bounding box of the specified text in point.
        /// </summary>
        public SizeF MeasureString(string text)
        {
            return MeasureString(text, UnitType.Point);
        }

        /// <summary>
        /// Gets or sets the font used for measurement.
        /// </summary>
        public MigraDoc.DocumentObjectModel.Font Font
        {
            get { return this.font; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (this.font != value)
                {
                    this.font = value;
                    this.gdiFont = null;
                }
            }
        }
        MigraDoc.DocumentObjectModel.Font font;

        /// <summary>
        /// Initializes appropriate GDI+ objects.
        /// </summary>
        Graphics Realize()
        {
            if (this.graphics == null)
                this.graphics = Graphics.FromHwnd(IntPtr.Zero);

            this.graphics.PageUnit = GraphicsUnit.Point;

            if (this.gdiFont == null)
            {
                System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
                if (this.font.Bold)
                    style |= System.Drawing.FontStyle.Bold;
                if (this.font.Italic)
                    style |= System.Drawing.FontStyle.Italic;

                this.gdiFont = new System.Drawing.Font(this.font.Name, this.font.Size, style, System.Drawing.GraphicsUnit.Point);
            }
            return this.graphics;
        }

        System.Drawing.Font gdiFont;
        System.Drawing.Graphics graphics;
    }
}
