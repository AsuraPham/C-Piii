using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Web;

namespace Codaxy.WkHtmlToPdf
{
    public class PdfConvertSection : ConfigurationSection
    {
        /// <summary>
        /// Path to wkhtmltopdf.exe binary
        /// </summary>
        [ConfigurationProperty("binaryPath", DefaultValue = "", IsRequired = false)]
        public string BinaryPath
        {
            get
            {
                return this["binaryPath"] as String;
            }
            set
            {
                this["binaryPath"] = value;
            }
        }

        /// <summary>
        /// Path to temporary folder
        /// </summary>
        [ConfigurationProperty("tempFolderPath", DefaultValue = "", IsRequired = false)]
        public String TempFolderPath
        {
            get
            {
                return this["tempFolderPath"] as String;
            }
            set
            {
                this["tempFolderPath"] = value;
            }
        }

        /// <summary>
        /// Timeout for executing wkhtmltopdf, in milliseconds.
        /// </summary>
        [ConfigurationProperty("timeout", DefaultValue = "60000", IsRequired = false)]
        public int Timeout
        {
            get
            {
                return (int)this["timeout"];
            }
            set
            {
                this["timeout"] = value;
            }
        }

        /// <summary>
        /// Additional command-line arguments for wkhtmltopdf.
        /// </summary>
        [ConfigurationProperty("arguments", DefaultValue = "--page-size A4 --margin-top 10 --margin-bottom 15 --header-spacing 5 --footer-spacing 5", IsRequired = false)]
        public string Arguments
        {
            get
            {
                return this["arguments"] as String;
            }
            set
            {
                this["arguments"] = value;
            }
        }

        /// <summary>
        /// Additional command-line arguments for wkhtmltopdf.
        /// </summary>
        [ConfigurationProperty("argumentsUrlReceipt", DefaultValue = "--page-size A4 --margin-top 10 --margin-bottom 15 --header-spacing 5 --footer-spacing 5", IsRequired = false)]
        public string ArgumentsUrlReceipt
        {
            get
            {
                return this["argumentsUrlReceipt"] as String;
            }
            set
            {
                this["argumentsUrlReceipt"] = value;
            }
        }
        /// <summary>
        /// Set false to block errors.
        /// </summary>
        [ConfigurationProperty("debug", DefaultValue = "true", IsRequired = false)]
        public bool Debug
        {
            get
            {
                return (bool)this["debug"];
            }
            set
            {
                this["debug"] = value;
            }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }

    public class PdfConvertArgumentSection : ConfigurationElement
    {

    }

    public class PdfConvertException : Exception
    {
        public PdfConvertException(String msg) : base(msg) { }
    }

    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.") { }
    }

    public class PdfOutput
    {
        public String OutputFilePath { get; set; }
        public Stream OutputStream { get; set; }
        public Action<PdfDocument, byte[]> OutputCallback { get; set; }
    }

    public class PdfDocument
    {
        public String Url { get; set; }
        public String HeaderUrl { get; set; }
        public String FooterUrl { get; set; }
        public object State { get; set; }
    }

    public class PdfConvert
    {
        static PdfConvertSection _e;

        public static PdfConvertSection Default
        {
            get
            {
                if (_e == null)
                {
                    _e = ConfigurationManager.GetSection("wkhtmltopdf") as PdfConvertSection;
                    if (_e == null)
                    {
                        _e = new PdfConvertSection();
                    }
                    if (String.IsNullOrEmpty(_e.BinaryPath))
                    {
                        _e.BinaryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"wkhtmltopdf\bin\wkhtmltopdf.exe");
                    }
                    if (String.IsNullOrEmpty(_e.TempFolderPath))
                    {
                        _e.TempFolderPath = Path.GetTempPath();
                    }
                }
                return _e;
            }
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output, string overwriteArgs = "")
        {
            ConvertHtmlToPdf(document, null, output, overwriteArgs);
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertSection environment, PdfOutput woutput, string overwriteArgs = "")
        {
            if (environment == null)
                environment = Default;

            String outputPdfFilePath;
            bool delete;
            if (woutput.OutputFilePath != null)
            {
                outputPdfFilePath = woutput.OutputFilePath;
                delete = false;
            }
            else
            {
                outputPdfFilePath = Path.Combine(environment.TempFolderPath, String.Format("{0}.pdf", Guid.NewGuid()));
                delete = true;
            }

            if (!File.Exists(environment.BinaryPath))
                throw new PdfConvertException(String.Format("File '{0}' not found. Check if wkhtmltopdf application is installed.", environment.BinaryPath));

            ProcessStartInfo si;

            StringBuilder paramsBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
            }
            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
            }

            if (!string.IsNullOrEmpty(overwriteArgs)){
                paramsBuilder.Append(overwriteArgs);
                paramsBuilder.Append(" ");
            }
            else
            {
                if (!String.IsNullOrEmpty(environment.Arguments))
                {
                    paramsBuilder.Append(environment.Arguments);
                    paramsBuilder.Append(" ");
                }
            }
            

            paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPdfFilePath);


            si = new ProcessStartInfo();
            si.CreateNoWindow = !environment.Debug;
            si.FileName = environment.BinaryPath;
            si.Arguments = paramsBuilder.ToString();
            si.UseShellExecute = false;
            si.RedirectStandardError = true;

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = si;
                    process.Start();
                    var stdErr = process.StandardError.ReadToEnd();

                    if (!process.WaitForExit(environment.Timeout))
                        throw new Exception(String.Format("timeout {0}", environment.Timeout));

                    if (!File.Exists(outputPdfFilePath))
                    {
                        if (process.ExitCode != 0)
                        {
                            var error = si.RedirectStandardError ? stdErr : String.Format("Process exited with code {0}.", process.ExitCode);
                            throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Wkhtmltopdf output: \r\n{1}", document.Url, error));
                        }

                        throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Reason: Output file '{1}' not found.", document.Url, outputPdfFilePath));
                    }

                    if (woutput.OutputStream != null)
                    {
                        using (Stream fs = new FileStream(outputPdfFilePath, FileMode.Open))
                        {
                            byte[] buffer = new byte[32 * 1024];
                            int read;

                            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                woutput.OutputStream.Write(buffer, 0, read);
                        }
                    }

                    if (woutput.OutputCallback != null)
                    {
                        woutput.OutputCallback(document, File.ReadAllBytes(outputPdfFilePath));
                    }

                    process.Close();
                }
            }
            finally
            {
                if (delete && File.Exists(outputPdfFilePath))
                {
                    try
                    {
                        // silently ignore delete error, otherwise it might overshadow the actual exception in the above code
                        File.Delete(outputPdfFilePath);
                    }
                    catch { }
                }
            }
        }
    }
}
