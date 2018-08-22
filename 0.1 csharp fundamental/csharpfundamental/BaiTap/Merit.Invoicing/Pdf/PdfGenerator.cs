using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Codaxy.WkHtmlToPdf;

namespace Merit.Invoicing.Pdf
{
    public class PdfGenerator
    {
        public void GenerateStandardUnloadingInvoice()
        {

        }

        public byte[] GetPdfByteStream(string url)
        {
            const string outputFileName = " - ";
            const string wkhtmlDir = "C:\\wkhtmltopdf\\";
            const string wkhtml = "wkhtmltopdf.exe";
            var p = new Process();

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = wkhtmlDir;
            p.StartInfo.FileName = wkhtml;

            string switches = "";
            switches += "--print-media-type ";
            switches += "--margin-top 10mm --margin-bottom 10mm --margin-right 10mm --margin-left 10mm ";
            switches += "--page-size Letter ";
            p.StartInfo.Arguments = switches + " " + url + " " + outputFileName;
            p.Start();

            //read output
            byte[] buffer = new byte[32768];
            byte[] file;
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    int read = p.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                    if (read <= 0)
                    {
                        break;
                    }
                    ms.Write(buffer, 0, read);
                }
                file = ms.ToArray();
            }

            // wait or exit
            p.WaitForExit(60000);

            // read the exit code, close process
            int returnCode = p.ExitCode;
            p.Close();

            return returnCode == 0 ? file : null;
        }

        public byte[] TryRunWkhtml(string url, string footerUrl = null, string overwriteArgs="")
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                PdfConvert.ConvertHtmlToPdf(new PdfDocument
                {
                    Url = url,
                    FooterUrl = footerUrl
                }
                        , new PdfOutput
                        {
                            OutputStream = ms,
                        }
                , overwriteArgs);

                bytes = ms.ToArray();
            }

            return bytes;
        }

    }
}