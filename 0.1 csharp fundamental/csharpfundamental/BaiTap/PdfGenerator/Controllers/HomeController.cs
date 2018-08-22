using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using PdfGenerator_conveer;

namespace PdfGenerator.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        [HttpPost]
        public ActionResult GetPDFByURL(string URL)
        {
            string validURL = new Uri(URL).ToString();
            //PDF Bytes from PDF Converter
            
            byte[] pdfFileBytes = new PdfGenerator_conveer.PdfGenerator().TryRunWkhtml(validURL);
            //Return Pdf To client
            return File(pdfFileBytes, "application/pdf");
        }
    }
}
