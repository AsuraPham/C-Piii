using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Merit.Invoicing
{
    public class InvoiceGenerationSettings
    {
        public String InvoiceGenerationBaseUrl { get; set; }
        public String KrogerInvoiceTemplatePath { get; set; }

        public InvoiceGenerationSettings()
        {
            InvoiceGenerationBaseUrl = ConfigurationManager.AppSettings["InvoiceGenerationBaseUrl"];
            KrogerInvoiceTemplatePath = ConfigurationManager.AppSettings["KrogerInvoiceTemplatePath"];
        }
    }
}