using System;
using System.IO;
using Kea.PDF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace Kea.Pdf.Test
{
    [TestClass]
    public class UnitTest1
    {
        class Row
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
        }

        static class Styles
        {
            public const string Normal = "Normal";
            public const string Heading1 = "Heading1";
            public static void DefineStyles(Document Document)
            {
                var st = Document.Styles[Normal];
                st.Font.Size = 8;
                st.Font.Bold = false;
                st.Font.Color = Colors.Black;
            }
        }

        internal Document GetDoc()
        {
            Document document = new Document();

            Styles.DefineStyles(document);

            Section section = document.AddSection();

            
            var Items = new[]
           {
                new Row { Nombre = "Rafael", Apellido = "Salguero" },
                new Row { Nombre =  "Alejandra", Apellido =  "Llanez " }
            };
            var Table = TablePrinter.Create(Items, new string[0]);

            var p = document.LastSection.AddParagraph();
            document.LastSection.Add(Table.Create());

            return document;
        }


        public static byte[] ToByte(Document Doc)
        {
            var renderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);

            renderer.Document = Doc;
            renderer.RenderDocument();

            MemoryStream S = new MemoryStream();
            renderer.PdfDocument.Save(S);

            return S.ToArray();
        }

        [TestMethod]
        public void GuardarPDF()
        {

            var Doc = GetDoc();
            var Data = UnitTest1.ToByte(Doc);
            System.IO.File.WriteAllBytes(@"C:\prueba.pdf", Data);
        }
    }
}
