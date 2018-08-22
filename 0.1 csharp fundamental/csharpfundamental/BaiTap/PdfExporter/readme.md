  
  Create asp.net project to view pdf file
  Using pdfGenerator and modify code to print with custom page-size
  // Su dung pdfGenerator va sua code de in duoc page-size theo kich co tu minh dua vao

  cai dat wkhtmltopdf theo version o image nay: "merit-print wkhtmltopdf.png" 
  
  [HttpGet]
        public FileContentResult UnloadingWorkOrderReceiptFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/UnloadingWorkOrderReceiptView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");
            return File(pdfByteStream, "application/pdf");
        }