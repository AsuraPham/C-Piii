using FirstLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstUnitTest
{
    [TestClass]
    public class StringHelperTest
    {
        PdfExporter pdfExporter;

        [TestInitialize]
        public void KhoiTao() {
             pdfExporter = new PdfExporter();

            // create records in db 
        }

        [TestCleanup]
        public void BoKhoiTao()
        {
            // clean records in db
        }

        [TestMethod]
        public void PasswordTest()
        {            
            var src = "sdfsfu";

            var xxx = new StringHelper().CreatePassword(src);

            Console.WriteLine(xxx);

            Assert.AreNotEqual(xxx, src);
        }


        [TestMethod]
        public void PasswordTestMinLenght()
        {
            var src = "sdfsfu";

            var xxx = new StringHelper().CreatePassword(src);

            Assert.IsTrue(xxx.Length>= 6);
        }


        [TestMethod]
        public void PasswordTestLessOneDigit()
        {
            var src = "sdfsfu";

            var xxx = new StringHelper().CreatePassword(src);

            var counter = 0;
            foreach(var x in xxx)
            {
                if (char.IsDigit(x))
                {
                    counter++;
                }
            }

            Assert.IsTrue(counter > 0);
        }
    }
}
