﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.Excel;
using Tonic.Excel.Printers;

namespace ObjectReports.Test
{
    [TestClass]
    public class ObjectReportsTest
    {
        [TestMethod]
        public void DateTimeTest()
        {
            var T = DateTimeTestAsync();
            T.Wait();
        }

        public async Task DateTimeTestAsync()
        {
            DateTime D = new DateTime(2015, 11, 23);
            var P = new MergePrinter(D);
            var bytes =   Export.SingleSheetExport("test", P, x=> { });

            //  System.IO.File.WriteAllBytes(@"c:\prueba\prueba.xlsx", bytes);
        }

        [TestMethod]
        public void TablePrinterTest()
        {
            var arr = Enumerable.Range(1, 10).Select(x => new
            {
                numero = x,
                n = (decimal?)null,
                d = (DateTime?)null,
                e = x.ToString()
            }).ToList();

            var p = TablePrinter.Create(arr);
        }
    }
}
