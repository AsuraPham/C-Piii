using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Merit.Invoicing.Pdf;

namespace TestGetPdfByteStream
{
    [TestClass]
    public class PdfConverTest
    {
        PdfGenerator pdfGenerator;
        [TestInitialize]
        public void Khoitao()
        {
            pdfGenerator = new PdfGenerator();
        }
        [TestCleanup]
        public void Huy()
        {
            // 
        }
        [TestMethod]
        public void TestMethod1()
        {
            byte by = 201;
            var zzz = new PdfGenerator().GetPdfByteStream("jhdjkhafk");
            Assert.AreEqual(by, zzz);   
        }
        [TestMethod]
        public void TestStringInput()
        {
            var url = "hhhhksdfksdfk";
            var xxx = new PdfGenerator().GetPdfByteStream(url);
            
        }
    }
}
