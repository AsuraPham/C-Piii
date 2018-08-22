using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Codaxy.WkHtmlToPdf;

namespace PdfGenerator_conveer
{
    public class PdfGenerator
    {
        public void GenerateStandardUnloadingInvoice()
        {

        }

        public byte[] GetPdfByteStream(string url)
        {
            //const string outputFileName = "";
            const string wkhtmlDir = "C:\\Program Files\\wkhtmltopdf\\bin\\";
            const string wkhtml = "wkhtmltopdf.exe";
            var p = new Process();

            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = wkhtmlDir;
            p.StartInfo.FileName = wkhtml;

            string switches = "";
            switches += "--print-media-type ";
            switches += "--margin-top 10mm --margin-bottom 10mm --margin-right 10mm --margin-left 10mm ";
            switches += "--page-size Letter";
            p.StartInfo.Arguments = switches + " " + url;
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

        public byte[] TryRunWkhtml(string url, string footerUrl = null)
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
                );

                bytes = ms.ToArray();
            }

            return bytes;
        }

    }
}


/*
Để thay đổi khổ giấy    switches += "--page-size Letter ";
sử dụng --page-size <Size> set paper size to: A4, letter,etc...(mặc định là khổ a4)
page-custom: sử dụng --page-height <mm>
                     --page-weight <mm>
Link nguồn: https://wkhtmltopdf.org/usage/wkhtmltopdf.txt

paperSize:
QPrinter::A0	5	841 x 1189 mm
QPrinter::A1	6	594 x 841 mm
QPrinter::A2	7	420 x 594 mm
QPrinter::A3	8	297 x 420 mm
QPrinter::A4	0	210 x 297 mm, 8.26 x 11.69 inches
QPrinter::A5	9	148 x 210 mm
QPrinter::A6	10	105 x 148 mm
QPrinter::A7	11	74 x 105 mm
QPrinter::A8	12	52 x 74 mm
QPrinter::A9	13	37 x 52 mm
QPrinter::B0	14	1000 x 1414 mm
QPrinter::B1	15	707 x 1000 mm
QPrinter::B2	17	500 x 707 mm
QPrinter::B3	18	353 x 500 mm
QPrinter::B4	19	250 x 353 mm
QPrinter::B5	1	176 x 250 mm, 6.93 x 9.84 inches
QPrinter::B6	20	125 x 176 mm
QPrinter::B7	21	88 x 125 mm
QPrinter::B8	22	62 x 88 mm
QPrinter::B9	23	33 x 62 mm
QPrinter::B10	16	31 x 44 mm
QPrinter::C5E	24	163 x 229 mm
QPrinter::Comm10E	25	105 x 241 mm, U.S. Common 10 Envelope
QPrinter::DLE	26	110 x 220 mm
QPrinter::Executive	4	7.5 x 10 inches, 190.5 x 254 mm
QPrinter::Folio	27	210 x 330 mm
QPrinter::Ledger	28	431.8 x 279.4 mm
QPrinter::Legal	3	8.5 x 14 inches, 215.9 x 355.6 mm
QPrinter::Letter	2	8.5 x 11 inches, 215.9 x 279.4 mm
QPrinter::Tabloid	29	279.4 x 431.8 mm
QPrinter::Custom	30	Unknown, or a user defined size.Custom(Size, Size)

Link:http://doc.qt.io/archives/qt-4.8/qprinter.html#PaperSize-enum
 */
