using System;
using System.Web.Mvc;
using Merit.Components;
using Merit.Components.Invoices.Domain.ReportModel;
using Merit.Components.Invoices.Domain.ReportModel.Activity;
using Merit.Components.Invoices.Domain.ReportModel.Department;
using Merit.Components.Invoices.Domain.ReportModel.MockData;
using Merit.Components.Invoices.Domain.ReportModel.Selection;
using Merit.Components.Invoices.Domain.ReportModel.Unloading;
using Merit.Components.Invoices.Domain.Repositories;
using Merit.Components.Invoices.Domain.Services;
using Merit.Components.Locations.Domain.Repositories;
using Merit.Invoicing.Excel;
using Merit.Invoicing.Pdf;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Merit.Components.WorkOrders.Domain.Repositories;
using Merit.Components.WorkOrders.Queries;
using System.Data.Entity;
using Merit.Components.Invoices.Domain.Entities;
using Merit.Components.WorkOrders.Domain.Entities;
using Merit.Components.Invoices.Domain.ReportModel.DLS;
using Merit.Components.Invoices.Queries.Projections;
using System.Configuration;
using Codaxy.WkHtmlToPdf;

namespace Merit.Invoicing.Controllers
{
    // TODO: merge Invoicing project into WebAPI ?
    public class GetController : Controller
    {
        private readonly InvoiceReportService invoiceReportService;

        private readonly KrogerExcelGenerator krogerExcelGenerator;

        private readonly InvoiceGenerationSettings invoiceGenerationSettings;

        private readonly AppSettings appSettings;

        private readonly PdfGenerator pdfGenerator;

        private readonly InvoiceRepository invoiceRepository;

        private readonly InvoiceService invoiceService;

        private readonly LocationRepository locationRepository;

        private readonly WorkOrderRepository workOrderRepository;

        private readonly UnloadingWorkOrderQueries unloadingWorkOrderQueries;

        private readonly DlsWorkOrderQueries dlsWorkOrderQueries;

        private readonly BOLQueries bolQueries;

        private readonly ServiceWorkOrderQueries serviceWorkOrderQueries;

        private readonly MeritDbContext dbContext;

        public GetController(
           MeritDbContext dbContext,
           InvoiceReportService invoiceReportService,
           AppSettings appSettings,
           InvoiceRepository invoiceRepository,
           InvoiceService invoiceService,
           LocationRepository locationRepository,
           WorkOrderRepository workOrderRepository,
           UnloadingWorkOrderQueries unloadingWorkOrderQueries,
           DlsWorkOrderQueries dlsWorkOrderQueries,
           BOLQueries bolQueries,
           ServiceWorkOrderQueries serviceWorkOrderQueries)
        {
            this.dbContext = dbContext;
            this.invoiceGenerationSettings = new InvoiceGenerationSettings();
            this.appSettings = appSettings;

            this.invoiceRepository = invoiceRepository;
            this.locationRepository = locationRepository;
            this.workOrderRepository = workOrderRepository;

            this.invoiceReportService = invoiceReportService;
            this.krogerExcelGenerator = new KrogerExcelGenerator(invoiceGenerationSettings.KrogerInvoiceTemplatePath);
            this.pdfGenerator = new PdfGenerator();

            this.unloadingWorkOrderQueries = unloadingWorkOrderQueries;
            this.dlsWorkOrderQueries = dlsWorkOrderQueries;
            this.invoiceService = invoiceService;
            this.bolQueries = bolQueries;
            this.serviceWorkOrderQueries = serviceWorkOrderQueries;
        }

        [HttpPost]
        public ActionResult SendInvoiceReport(IList<Guid> invoiceIds)
        {
            if (invoiceIds == null || invoiceIds.Count == 0)
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "No invoices selected" }, JsonRequestBehavior.AllowGet);
            }

            var invoiceList = invoiceRepository.FindByIds(invoiceIds);
            var sendResults = invoiceList.AsParallel().WithDegreeOfParallelism(10).Select(SendInvoice).ToList();

            // Solves net::ERR_RESPONSE_HEADERS_MULTIPLE_CONTENT_DISPOSITION error
            Response.ClearHeaders();

            return Json(new
            {
                success = true,
                results = sendResults
            });
        }

        private InvoiceSendResult SendInvoice(Invoice invoice)
        {
            try
            {
                IList<string> recipients = null;
                if (invoice.BillTo == BillTo.Client)
                {
                    if (!string.IsNullOrEmpty(invoice.Client?.EmailRecipients))
                    {
                        recipients = JsonConvert.DeserializeObject<List<string>>(invoice.Client.EmailRecipients);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(invoice.Carrier?.ReceiptEmail))
                    {
                        recipients = JsonConvert.DeserializeObject<List<string>>(invoice.Carrier.ReceiptEmail);
                    }
                }

                if (recipients == null || recipients.Count == 0)
                {
                    return new InvoiceSendResult($"No recipients for e-Invoice#: {invoice.InvoiceNumber}");
                }

                var file = (FileContentResult)InvoiceReport(invoice.Id, invoice.FormatType);

                var message = new MailMessage
                {
                    Body = @"<p>We appreciate your business.</p><p>Sincerely,</p>
                             <p><a href='http://www.meritlogistics.com' target='_blank'>Merit Integrated Logistics</a></p>",
                    From = new MailAddress(appSettings.InvoiceSenderEmail),
                    IsBodyHtml = true,
                    Subject = $"Merit Integrated Logistic's e-Invoice #{invoice.InvoiceNumber} {invoice.ServiceTo.Value.ToShortDateString()}",
                };

                foreach (var email in recipients)
                {
                    message.To.Add(email);
                }

                var invoiceBccEmail = appSettings.InvoiceBccEmail;
                if (!string.IsNullOrWhiteSpace(invoiceBccEmail))
                {
                    message.Bcc.Add(invoiceBccEmail);
                }

                message.Attachments.Add(
                    new Attachment(new System.IO.MemoryStream(file.FileContents), file.FileDownloadName + ".pdf", file.ContentType)
                );

                var smtpClient = new SmtpClient();
                smtpClient.Send(message);

                invoiceService.LockInvoice(invoice);
                return new InvoiceSendResult(string.Join(", ", message.To.Select(s => s.Address).ToList()),
                    message.Subject,
                    Url.Action("InvoiceReport", "Get", new { invoiceId = invoice.Id, formatType = invoice.FormatType }));
            }
            catch (Exception)
            {
                var message = $"Failed sending e-Invoice#: {invoice.InvoiceNumber}";
                return new InvoiceSendResult(message);
            }
        }

        [HttpGet]
        public ActionResult InvoiceReport(Guid invoiceId, InvoiceFormatType formatType, FileFormat fileFormat = FileFormat.Pdf)
        {
            // TODO: refactor into several classes (IInvoiceReportViewProvider?)
            switch (fileFormat)
            {
                case FileFormat.Html:
                    switch (formatType)
                    {
                        case InvoiceFormatType.Activity:
                            return ActivityInvoiceView(invoiceId);
                        case InvoiceFormatType.SCORS:
                        case InvoiceFormatType.Department:
                            return DepartmentInvoiceView(invoiceId);
                        case InvoiceFormatType.Unloading:
                            return UnloadingInvoiceView(invoiceId);
                        case InvoiceFormatType.Selection:
                            return SelectionInvoiceView(invoiceId);
                        case InvoiceFormatType.DLS:
                            return DlsInvoiceView(invoiceId);
                    }
                    break;
                case FileFormat.Pdf:
                    switch (formatType)
                    {
                        case InvoiceFormatType.Activity:
                            return ActivityInvoiceFile(invoiceId);
                        case InvoiceFormatType.SCORS:
                        case InvoiceFormatType.Department:
                            return DepartmentInvoiceFile(invoiceId);
                        case InvoiceFormatType.Unloading:
                            return UnloadingInvoiceFile(invoiceId);
                        case InvoiceFormatType.Selection:
                            return SelectionInvoiceFile(invoiceId);
                        case InvoiceFormatType.DLS:
                            return DlsInvoiceFile(invoiceId);
                    }
                    break;
                case FileFormat.Excel:
                    switch (formatType)
                    {
                        case InvoiceFormatType.Activity:
                            return ActivityInvoiceExcel(invoiceId);
                        case InvoiceFormatType.SCORS:
                        case InvoiceFormatType.Department:
                            return DepartmentInvoiceExcel(invoiceId);
                        case InvoiceFormatType.Unloading:
                            return UnloadingInvoiceExcel(invoiceId);
                        case InvoiceFormatType.Selection:
                            return SelectionInvoiceExcel(invoiceId);
                        case InvoiceFormatType.DLS:
                            return DlsInvoiceExcel(invoiceId);
                    }
                    break;
            }

            throw new Exception(
                $"Unsupported combination of Invoice Format ({formatType}) and File Format ({fileFormat})!");
        }

        [HttpGet]
        public FileContentResult KrogerInvoiceExcelReport(int locationId, DateTime periodFrom, DateTime periodTo, Boolean breakdownByDepartment)
        {
            var location = locationRepository.FindById(locationId);
            if (location == null)
            {
                throw new Exception("Location not found");
            }
            var workOrders = workOrderRepository.GetLoadsForKrogerInvoice(location, periodFrom, periodTo);
            var invoiceReportModel = new KrogerUnloadingInvoiceModel(location, workOrders, periodFrom, periodTo, breakdownByDepartment);

            var fileBytes = krogerExcelGenerator.Generate(invoiceReportModel);

            var invoiceFileName = $"Invoice{invoiceReportModel.HeaderInfo.InvoiceNumber}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", invoiceFileName);
        }

        #region Unloading Invoice

        [HttpGet]
        public FileContentResult UnloadingInvoiceFile(Guid invoiceId, Boolean mockData = false)
        {
            var url = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "Get/UnloadingInvoiceView?invoiceId=" + invoiceId;

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition", "inline; filename = invoice.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public FileContentResult UnloadingInvoiceExcel(Guid invoiceId, Boolean mockData = false)
        {
            var invoiceReportModel = invoiceReportService
                    .GetOrCreateReport(invoiceId, InvoiceFormatType.Unloading)
                    as UnloadingInvoiceModel;

            invoiceReportModel.AllWorkOrdersInfo = invoiceRepository.FindById(invoiceId).InvoiceLineItem.Select(i => unloadingWorkOrderQueries.FindOverviewById(i.WorkOrder.Id)).OrderBy(i => i.Start.Value);

            var excelGenerator = new ExportInvoiceReportToExcel();

            var fileBytes = excelGenerator.GenerateUnloadingInvoiceReport(invoiceReportModel);

            var reportFileName = String.Format("UnloadingInvoiceReport.xlsx");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportFileName);
        }

        [HttpGet]
        public ActionResult UnloadingInvoiceView(Guid invoiceId, Boolean mockData = false)
        {
            var invoiceReportModel = invoiceReportService
                    .GetOrCreateReport(invoiceId, InvoiceFormatType.Unloading)
                    as UnloadingInvoiceModel;

            if (invoiceReportModel == null)
                return Content("Invoice not found.");

            invoiceReportModel.AllWorkOrdersInfo = invoiceRepository.FindById(invoiceId).InvoiceLineItem.Select(i => unloadingWorkOrderQueries.FindOverviewById(i.WorkOrder.Id)).OrderBy(i => i.Start.Value);
            return View("~/Views/Invoice/StandardUnloading.cshtml", invoiceReportModel);
        }

        #endregion

        #region Activity Invoice

        [HttpGet]
        public FileContentResult ActivityInvoiceFile(Guid invoiceId, Boolean mockData = false)
        {
            var url = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "Get/ActivityInvoiceView";

            var args = $"?invoiceId={invoiceId}&mockData={mockData}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url + args, footerUrl);

            Response.AppendHeader("Content-Disposition", "inline; filename = activity_invoice.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public FileContentResult ActivityInvoiceExcel(Guid invoiceId, Boolean mockData = false)
        {
            var invoiceModel = mockData
                    ? MockInvoiceData.MockActivityInvoice()
                    : invoiceReportService
                            .GetOrCreateReport(invoiceId, InvoiceFormatType.Activity)
                            as ActivityInvoiceModel;

            var excelGenerator = new ExportInvoiceReportToExcel();

            var fileBytes = excelGenerator.GenerateActivityInvoiceReport(invoiceModel);

            var reportFileName = String.Format("ActivityInvoiceReport.xlsx");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportFileName);
        }

        [HttpGet]
        public ActionResult ActivityInvoiceView(Guid invoiceId, Boolean mockData = false)
        {
            var invoiceModel = mockData
                    ? MockInvoiceData.MockActivityInvoice()
                    : invoiceReportService
                            .GetOrCreateReport(invoiceId, InvoiceFormatType.Activity)
                            as ActivityInvoiceModel;

            return View("~/Views/Invoice/Activity.cshtml", invoiceModel);
        }

        #endregion

        #region Department Invoice

        [HttpGet]
        public FileContentResult DepartmentInvoiceFile(Guid invoiceId, Boolean mockData = false)
        {
            var url = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "Get/DepartmentInvoiceView";

            var args = $"?invoiceid={invoiceId}&mockdata={mockData}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url + args, footerUrl);

            Response.AppendHeader("Content-Disposition", "inline; filename = department_invoice.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public FileContentResult DlsInvoiceFile(Guid invoiceId)
        {
            var url = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "Get/DlsInvoiceView";

            var args = $"?invoiceid={invoiceId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            // TODO: get rid of code duplication
            var pdfByteStream = pdfGenerator.TryRunWkhtml(url + args, footerUrl);

            Response.AppendHeader("Content-Disposition", "inline; filename = dls_invoice.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public FileContentResult DepartmentInvoiceExcel(Guid invoiceId, Boolean mockData = false)
        {
            var model = mockData
                    ? MockInvoiceData.MockDepartmentInvoice()
                    : invoiceReportService
                            .GetOrCreateReport(invoiceId, InvoiceFormatType.Department)
                            as DepartmentInvoiceModel;

            var excelGenerator = new ExportInvoiceReportToExcel();

            var fileBytes = excelGenerator.GenerateDepartmentInvoiceReport(model);

            var reportFileName = String.Format("DepartmentInvoiceReport.xlsx");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportFileName);
        }

        [HttpGet]
        public FileContentResult DlsInvoiceExcel(Guid invoiceId, Boolean mockData = false)
        {
            var model = invoiceReportService
                            .GetOrCreateReport(invoiceId, InvoiceFormatType.DLS)
                            as DlsInvoiceModel;

            var excelGenerator = new ExportInvoiceReportToExcel();

            var fileBytes = excelGenerator.GenerateDlsInvoiceReport(model);

            var reportFileName = String.Format("DlsInvoiceReport.xlsx");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportFileName);
        }

        [HttpGet]
        public ViewResult DepartmentInvoiceView(Guid invoiceId, Boolean mockData = false)
        {
            var model = mockData
                    ? MockInvoiceData.MockDepartmentInvoice()
                    : invoiceReportService
                            .GetOrCreateReport(invoiceId, InvoiceFormatType.Department)
                            as DepartmentInvoiceModel;

            return View("~/Views/Invoice/Department.cshtml", model);
        }

        [HttpGet]
        public ViewResult DlsInvoiceView(Guid invoiceId)
        {
            var model = invoiceReportService
                            .GetOrCreateReport(invoiceId, InvoiceFormatType.DLS)
                            as DlsInvoiceModel;

            return View("~/Views/Invoice/DLS.cshtml", model);
        }

        #endregion

        #region Selection Invoice

        [HttpGet]
        public FileContentResult SelectionInvoiceFile(Guid invoiceId, Boolean mockData = false)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/SelectionInvoiceView?invoiceid={invoiceId}&mockdata={mockData}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition", "inline; filename = selection_invoice.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public FileContentResult SelectionInvoiceExcel(Guid invoiceId, Boolean mockData = false)
        {
            var model = mockData
                    ? MockInvoiceData.MockSelectionInvoice()
                    : invoiceReportService.GetOrCreateReport(invoiceId, InvoiceFormatType.Selection)
                    as SelectionInvoiceModel;

            var excelGenerator = new ExportInvoiceReportToExcel();

            var fileBytes = excelGenerator.GenerateSelectionInvoiceReport(model);

            var reportFileName = String.Format("SelectionInvoiceReport.xlsx");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportFileName);
        }

        [HttpGet]
        public ViewResult SelectionInvoiceView(Guid invoiceId, Boolean mockData = false)
        {
            var model = mockData
                    ? MockInvoiceData.MockSelectionInvoice()
                    : invoiceReportService.GetOrCreateReport(invoiceId, InvoiceFormatType.Selection)
                    as SelectionInvoiceModel;

            return View("~/Views/Invoice/Selection.cshtml", model);
        }

        #endregion

        #region Work Orders

        [HttpGet]
        public FileContentResult UnloadingWorkOrderOverviewFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/UnloadingWorkOrderOverviewView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");

            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public ViewResult UnloadingWorkOrderOverviewView(Guid workOrderId)
        {
            var order = unloadingWorkOrderQueries.FindOverviewById(workOrderId);

            return View("~/Views/WorkOrder/UnloadingOverview.cshtml", order);
        }

        [HttpGet]
        public FileContentResult DlsWorkOrderOverviewFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/DlsWorkOrderOverviewView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");

            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public ViewResult DlsWorkOrderOverviewView(Guid workOrderId)
        {
            var order = dlsWorkOrderQueries.FindOverview(workOrderId, true);
            return View("~/Views/WorkOrder/DlsOverview.cshtml", order);
        }

        [HttpGet]
        public FileContentResult UnloadingWorkOrderReceiptFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/UnloadingWorkOrderReceiptView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var _conf = ConfigurationManager.GetSection("wkhtmltopdf") as PdfConvertSection;
            var overwriteAgrs = _conf.ArgumentsUrlReceipt;
            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl,overwriteAgrs);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public ViewResult UnloadingWorkOrderReceiptView(Guid workOrderId)
        {
            var order = unloadingWorkOrderQueries.FindUnloadingWorkOrderReceipt(workOrderId);
            ViewBag.PartialView = "_UnloadingWorkOrderReceipt";
            return View("~/Views/WorkOrder/UnloadingReceipt.cshtml", order);
        }

        public FileContentResult BolWorkOrderFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/BolWorkOrderView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public ViewResult BolWorkOrderView(Guid workOrderId)
        {
            var model = bolQueries.FindBolWordOrder(workOrderId);

            ViewBag.PartialView = "_BolPartial";
            return View("~/Views/WorkOrder/BolView.cshtml", model);
        }

        public FileContentResult WHServiceFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/WHServiceView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public ViewResult WHServiceView(Guid workOrderId)
        {
            var model = serviceWorkOrderQueries.FindWarehouseServicePrint(workOrderId);

            ViewBag.PartialView = "_WHServicePartial";
            return View("~/Views/WorkOrder/WHServiceView.cshtml", model);
        }


        [HttpGet]
        public FileContentResult DlsWorkOrderReceiptFile(Guid workOrderId)
        {
            var url =
                $"{invoiceGenerationSettings.InvoiceGenerationBaseUrl}Get/DlsWorkOrderReceiptView?workOrderId={workOrderId}";

            var footerUrl = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "content/footer.html";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url, footerUrl);

            Response.AppendHeader("Content-Disposition",
                $"inline; filename = {workOrderRepository.GetNumberById(workOrderId)}.pdf");
            return File(pdfByteStream, "application/pdf");
        }

        [HttpGet]
        public ViewResult DlsWorkOrderReceiptView(Guid workOrderId)
        {
            var order = dlsWorkOrderQueries.FindWorkOrderReceipt(workOrderId);

            ViewBag.PartialView = "_DlsWorkOrderReceipt";
            return View("~/Views/WorkOrder/DlsReceipt.cshtml", order);
        }

        #endregion

        #region Customers

        [HttpGet]
        public ActionResult SearchCustomersView(string searchText, int? rateGroupId = null, bool? payViaInvoice = null, bool? active = null)
        {
            var query = (from dbCustomer in dbContext.Customers
                         where !dbCustomer.Deleted
                         select dbCustomer);

            query = query.Include(c => c.Address);
            query = query.Include(c => c.RateGroups);
            query = query.Include(c => c.RateGroups.Select(r => r.FactReference));
            query = query.Include(c => c.RateGroups.Select(r => r.FactReference.Fact));

            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(searchText));
            }

            if (rateGroupId.HasValue)
            {
                query = query.Where(c => c.RateGroups.Any(rg => rg.FactReference.FactId == rateGroupId));
            }

            if (payViaInvoice.HasValue)
            {
                query = query.Where(c => c.PayViaInvoice == payViaInvoice.Value);
            }

            if (active.HasValue)
            {
                query = query.Where(c => c.Active == active.Value);
            }

            query = query.OrderByDescending(c => c.Active).ThenBy(c => c.Name);

            return View("~/Views/Customers/CustomerListReport.cshtml", query.ToList());
        }

        [HttpGet]
        public FileContentResult SearchCustomersPdf(string searchText, int? rateGroupId = null, bool? payViaInvoice = null, bool? active = null)
        {
            var url = invoiceGenerationSettings.InvoiceGenerationBaseUrl + "Get/SearchCustomersView";

            var args =
                $"?searchText={searchText}&rateGroupId={rateGroupId}&payViaInvoice={payViaInvoice}&active={active}";

            var pdfByteStream = pdfGenerator.TryRunWkhtml(url + args);

            return File(pdfByteStream, "application/pdf", $"customers_{DateTime.Today:yyyyMMdd}.pdf");
        }

        #endregion
    }

    public enum FileFormat
    {
        Html, Pdf, Excel
    }
}
