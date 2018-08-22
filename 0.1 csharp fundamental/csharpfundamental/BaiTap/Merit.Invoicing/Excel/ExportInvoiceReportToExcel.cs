using Merit.Components.Invoices.Domain.ReportModel.Unloading;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using Merit.Components.Invoices.Domain.ReportModel.Department;
using Merit.Components.Invoices.Domain.ReportModel.Activity;
using Merit.Components.Invoices.Domain.ReportModel.Selection;
using Merit.Components.Invoices.Domain.ReportModel.DLS;

namespace Merit.Invoicing.Excel
{
    public class ExportInvoiceReportToExcel
    {
        private readonly string invoiceGenerationBaseUrl;

        public ExportInvoiceReportToExcel()
        {
            invoiceGenerationBaseUrl = ConfigurationManager.AppSettings["InvoiceGenerationBaseUrl"];
        }

        public byte[] GenerateUnloadingInvoiceReport(UnloadingInvoiceModel model)
        {
            var templatePath = ConfigurationManager.AppSettings["UnloadingInvoiceTemplatePath"];
            var template = new FileInfo(templatePath);
            using (var package = new ExcelPackage(template, true))
            {
                var sheet = package.Workbook.Worksheets[1];

                #region Header

                sheet.Cells[13, 1].Value = model.HeaderInfo.Client;
                if (!string.IsNullOrWhiteSpace(model.HeaderInfo.ClientLocation))
                    sheet.Cells[13, 1].Value += " - " + model.HeaderInfo.ClientLocation;

                if(!string.IsNullOrEmpty(model.HeaderInfo.NameForReceipts))
                    sheet.Cells[14, 1].Value = "Attn: " + model.HeaderInfo.NameForReceipts;

                sheet.Cells[15, 1].Value = model.HeaderInfo.AddressLine1;
                sheet.Cells[16, 1].Value = model.HeaderInfo.AddressLine2;
                sheet.Cells[17, 1].Value = model.HeaderInfo.AddressLine3;

                sheet.Cells[12, 11].Value = model.HeaderInfo.InvoiceNumber;
                sheet.Cells[12, 12].Value = model.HeaderInfo.InvoiceDate;
                sheet.Cells[14, 11].Value = model.HeaderInfo.InvoiceAmount;
                sheet.Cells[14, 12].Value = model.HeaderInfo.DueDate;

                sheet.Cells[17, 11].Value = model.HeaderInfo.InvoiceAddress1;
                sheet.Cells[18, 11].Value = model.HeaderInfo.InvoiceAddress2;

                #endregion

                #region Main

                var row = 24;
                foreach (var workOrder in model.AllWorkOrdersByDate)
                {
                    sheet.InsertRow(row, 1, row + 1);
                    sheet.Cells[row, 1].Value = workOrder.LoadDate;
                    sheet.Cells[row, 2].Value = workOrder.WorkOrderNumber;
                    sheet.Cells[row, 3].Value = workOrder.PurchaseOrderNumber;
                    sheet.Cells[row, 4].Value = workOrder.TrailerNumber;
                    sheet.Cells[row, 5].Value = workOrder.Vendor;
                    sheet.Cells[row, 6].Value = workOrder.Carrier;
                    sheet.Cells[row, 7].Value = workOrder.Description;
                    sheet.Cells[row, 8].Value = workOrder.StockKeepingUnit;
                    sheet.Cells[row, 9].Value = workOrder.Quantity;
                    sheet.Cells[row, 10].Value = workOrder.UnitOfMeasure;
                    sheet.Cells[row, 11].Value = workOrder.Rate;
                    sheet.Cells[row, 12].Value = workOrder.TotalAmount;
                    row++;
                }

                sheet.Cells[row, 1, row, 12].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.None);
                sheet.Cells[row + 1, 12].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", model.AllWorkOrdersByDate.Count + 1);

                #endregion

                #region WorkOrders

                var templateWorkOrderSheet = package.Workbook.Worksheets["TemplateWorkOrder"];
                if (model.AllWorkOrdersInfo != null)
                {
                    foreach (var workOrder in model.AllWorkOrdersInfo)
                    {
                        var sheetName = workOrder.WoNumber;
                        var c = 2;
                        while (package.Workbook.Worksheets.Any(a => a.Name.Equals(sheetName)))
                        {
                            sheetName = string.Format("{0} ({1})", workOrder.WoNumber, c++);
                        }
                        var currentWorkOrderSheet = package.Workbook.Worksheets.Add(sheetName, templateWorkOrderSheet);

                        putImageToWorksheet(currentWorkOrderSheet, getImage(invoiceGenerationBaseUrl + @"Content/Images/New_MeritLogo.png"), 0, 0, 90);

                        var rowIndex = 1;
                        currentWorkOrderSheet.Cells[rowIndex, 8].Value = workOrder.WoNumber;

                        rowIndex++; //rowIndex = 2;
                        currentWorkOrderSheet.Cells[rowIndex, 8].Value = workOrder.DepartmentName;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.Start;

                        rowIndex++; //rowIndex = 3;
                        currentWorkOrderSheet.Cells[rowIndex, 8].Value = workOrder.ShiftName;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.Finish;

                        rowIndex++; //rowIndex = 4;
                        currentWorkOrderSheet.Cells[rowIndex, 8].Value = workOrder.Created;

                        rowIndex++; //rowIndex = 5;
                        currentWorkOrderSheet.Cells[rowIndex, 2].Value = workOrder.PoNumber;
                        currentWorkOrderSheet.Cells[rowIndex, 5].Value = workOrder.ProductNumber;
                        currentWorkOrderSheet.Cells[rowIndex, 7].Value = workOrder.DoorNumber;
                        currentWorkOrderSheet.Cells[rowIndex, 9].Value = workOrder.TruckNumber;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.TrailerNumber;

                        rowIndex++; //rowIndex = 6;
                        currentWorkOrderSheet.Cells[rowIndex, 2].Value = workOrder.CarrierName;
                        currentWorkOrderSheet.Cells[rowIndex, 6].Value = workOrder.VendorName;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.DeptCodeNumber;

                        rowIndex++; //rowIndex = 7;
                        currentWorkOrderSheet.Cells[rowIndex, 2].Value = workOrder.ActivityName;
                        currentWorkOrderSheet.Cells[rowIndex, 5].Value = workOrder.UnitOfMeasure;
                        currentWorkOrderSheet.Cells[rowIndex, 8].Value = workOrder.BillTo;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.PalletType;

                        rowIndex++; //rowIndex = 8;
                        currentWorkOrderSheet.Cells[rowIndex, 2].Value = workOrder.StartPalletCount;
                        currentWorkOrderSheet.Cells[rowIndex, 5].Value = workOrder.EndPalletCount;
                        currentWorkOrderSheet.Cells[rowIndex, 8].Value = workOrder.CaseCount;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.ItemCount;

                        rowIndex++; //rowIndex = 9;
                        currentWorkOrderSheet.Cells[rowIndex, 3].Value = workOrder.SpecialInstructions;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.UnloadingTotal;

                        rowIndex += 2; //rowIndex = 11;
                        foreach (var charge in workOrder.AdditionalCharges)
                        {
                            currentWorkOrderSheet.InsertRow(rowIndex, 1, rowIndex + 1);
                            currentWorkOrderSheet.Cells[rowIndex + 1, 1, rowIndex + 1, 11].Copy(currentWorkOrderSheet.Cells[rowIndex, 1]);
                            currentWorkOrderSheet.Cells[rowIndex, 2].Value = charge.Name;
                            currentWorkOrderSheet.Cells[rowIndex, 6].Value = charge.Rate;
                            currentWorkOrderSheet.Cells[rowIndex, 9].Value = charge.Quantity;
                            currentWorkOrderSheet.Cells[rowIndex, 11].Value = charge.Price;
                            rowIndex++;
                        }
                        currentWorkOrderSheet.DeleteRow(rowIndex);

                        currentWorkOrderSheet.Cells[rowIndex, 2].Value = workOrder.PaymentType;
                        currentWorkOrderSheet.Cells[rowIndex, 6].Value = workOrder.ComCheckNumber;
                        currentWorkOrderSheet.Cells[rowIndex, 9].Value = workOrder.PaymentAuthorizationCode;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.ServiceCharge;

                        rowIndex++;
                        currentWorkOrderSheet.Cells[rowIndex, 2].Value = workOrder.DriverName;
                        currentWorkOrderSheet.Cells[rowIndex, 11].Value = workOrder.TotalBill;
                        rowIndex++;

                        if (workOrder.HasDriverSignature)
                        {
                            var driverSignImg = getImage(workOrder.DriverSignatureUrl);
                            putImageToWorksheet(currentWorkOrderSheet, driverSignImg, rowIndex, 0, 90);
                        }

                        if (workOrder.HasSupervisorSignature)
                        {
                            var supervisorSignImg = getImage(workOrder.SupervisorSignatureUrl);
                            putImageToWorksheet(currentWorkOrderSheet, supervisorSignImg, rowIndex, 6, 90);
                        }

                        foreach (var worker in workOrder.WorkerAssignments)
                        {
                            currentWorkOrderSheet.Cells[rowIndex + 7, 1].Value = string.Format("{0}, {1} {2}", worker.LastName, worker.FirstName, worker.MiddleName);
                            rowIndex++;
                        }
                    }
                }
                package.Workbook.Worksheets.Delete(templateWorkOrderSheet); 

                #endregion

                return package.GetAsByteArray();
            }
        }

        // TODO: refactor into separate classes
        public byte[] GenerateDepartmentInvoiceReport(DepartmentInvoiceModel model)
        {
            var templatePath = ConfigurationManager.AppSettings["DepartmentInvoiceTemplatePath"];
            var template = new FileInfo(templatePath);
            using (var package = new ExcelPackage(template, true))
            {
                var sheet = package.Workbook.Worksheets[1];

                #region Header

                sheet.Cells[13, 1].Value = model.HeaderInfo.Client;
                if (!string.IsNullOrWhiteSpace(model.HeaderInfo.ClientLocation))
                    sheet.Cells[13, 1].Value += " - " + model.HeaderInfo.ClientLocation;

                if (!string.IsNullOrEmpty(model.HeaderInfo.NameForReceipts))
                    sheet.Cells[14, 1].Value = "Attn: " + model.HeaderInfo.NameForReceipts;

                sheet.Cells[15, 1].Value = model.HeaderInfo.AddressLine1;
                sheet.Cells[16, 1].Value = model.HeaderInfo.AddressLine2;
                sheet.Cells[17, 1].Value = model.HeaderInfo.AddressLine3;

                sheet.Cells[12, 5].Value = model.HeaderInfo.InvoiceNumber;
                sheet.Cells[12, 6].Value = model.HeaderInfo.InvoiceDate;
                sheet.Cells[14, 5].Value = model.HeaderInfo.InvoiceAmount;
                sheet.Cells[14, 6].Value = model.HeaderInfo.DueDate;

                sheet.Cells[17, 5].Value = model.HeaderInfo.InvoiceAddress1;
                sheet.Cells[18, 5].Value = model.HeaderInfo.InvoiceAddress2;

                #endregion

                #region Main

                var shifts = model.DailyActivities.Any(x => x.Any())
                    ? model.DailyActivities.First().First().Shifts
                    : new List<ShiftActivityModel>();

                for (int i = 0; i < shifts.Count-1; i++)
                {
                    sheet.InsertColumn(3, 1, 4);
                    sheet.Cells[21, 4, 24, 4].Copy(sheet.Cells[21, 3, 24, 3]);
                }

                for (int i = 0; i < shifts.Count; i++)
                {
                    sheet.Cells[23, 3 + i].Value = shifts[i].ShiftName;
                }


                for (int i = 0; i < model.DailyActivities.Count - 1; i++)
                {
                    sheet.Cells[21, 1, 25, 5 + shifts.Count].Copy(sheet.Cells[27 + i * 6, 1]);
                }

                var tableIndex = 0;
                var totalRows = new List<int>();
                foreach (var activityGroup in model.DailyActivities)
                {
                    sheet.Cells[21 + tableIndex, 1].Value = model.Invoice.DepartmentName + ", " + activityGroup.First().ActivityType;

                    var row = 24 + tableIndex;
                    foreach (var day in activityGroup)
                    {
                        sheet.InsertRow(row, 1, row + 1);
                        sheet.Cells[row, 1].Value = day.Date.DayOfWeek;
                        sheet.Cells[row, 2].Value = day.Date;

                        var columnIndex = 3;
                        foreach (var shift in day.Shifts)
                        {
                            sheet.Cells[row, columnIndex].Value = shift.NumberOfUnits;
                            columnIndex++;
                        }
                        sheet.Cells[row, columnIndex].Value = day.TotalUnits;
                        sheet.Cells[row, columnIndex + 1].Value = day.Rate;
                        sheet.Cells[row, columnIndex + 2].Value = day.TotalAmount;
                        row++;
                    }

                    sheet.Cells[row, 1, row, 5 + shifts.Count].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.None);
                    sheet.Cells[row + 1, 3 + shifts.Count].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", activityGroup.Count + 1);
                    sheet.Cells[row + 1, 5 + shifts.Count].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", activityGroup.Count + 1);
                    totalRows.Add(row + 1);

                    tableIndex += 6 + activityGroup.Count;
                }

                sheet.Cells[21 + tableIndex, 3 + shifts.Count].Value = "Grand Total:";
                sheet.Cells[21 + tableIndex, 5 + shifts.Count].Formula = 
                    string.Join("+", totalRows.Select(s => string.Format("{0}{1}", string.Concat(sheet.Cells[21 + tableIndex, 5 + shifts.Count].Address.TakeWhile(char.IsLetter)), s)));
                sheet.Cells[21 + tableIndex, 5 + shifts.Count].Style.Numberformat.Format = @"\$##,##0.00";
                sheet.Cells[21 + tableIndex, 3 + shifts.Count, 21 + tableIndex, 5 + shifts.Count].Style.Font.Bold = true;

                #endregion

                #region WorkOrders

                var woSheet = package.Workbook.Worksheets[2];

                if (model.ScorsWorkOrders.Count > 0)
                {
                    woSheet.Cells[2, 2].Value = "Week Ending: " + model.HeaderInfo.ServiceDateEnd;

                    var woRow = 5;
                    foreach (var wo in model.ScorsWorkOrders.OrderBy(wo => wo.Date))
                    {
                        var purchaseOrders = wo.PurchaseOrders;
                        for (var i = 0; i < purchaseOrders.Count; i++)
                        {
                            woSheet.InsertRow(woRow, 1, woRow + 1);
                            if (i == 0)
                            {
                                woSheet.Cells[woRow, 1].Value = wo.Date;
                                woSheet.Cells[woRow, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                woSheet.Cells[woRow, 2].Value = wo.WorkOrderNumber;
                                woSheet.Cells[woRow, 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }
                            woSheet.Cells[woRow, 3].Value = purchaseOrders[i].NumberOfCases;
                            woSheet.Cells[woRow, 4].Value = purchaseOrders[i].Number;
                            woSheet.Cells[woRow, 5].Value = purchaseOrders[i].Quantity;
                            woSheet.Cells[woRow, 6].Value = purchaseOrders[i].Amount;
                            woRow++;
                        }
                        if (purchaseOrders.Count > 1)
                        {
                            woSheet.Cells[woRow - purchaseOrders.Count, 1, woRow - 1, 1].Merge = true;
                            woSheet.Cells[woRow - purchaseOrders.Count, 2, woRow - 1, 2].Merge = true;
                        }
                    }

                    woSheet.Cells[woRow + 1, 6].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", model.ScorsWorkOrders.SelectMany(s => s.PurchaseOrders).Count() + 1);
                }
                else
                {
                    package.Workbook.Worksheets.Delete(woSheet);
                }

                #endregion

                return package.GetAsByteArray();
            }
        }

        // TODO: refactor into separate classes
        public byte[] GenerateDlsInvoiceReport(DlsInvoiceModel model)
        {
            // for now re-use template for Department Invoice
            var templatePath = ConfigurationManager.AppSettings["DepartmentInvoiceTemplatePath"];
            var template = new FileInfo(templatePath);
            using (var package = new ExcelPackage(template, true))
            {
                var sheet = package.Workbook.Worksheets[1];

                #region Header

                // TODO: factor out header
                sheet.Cells[13, 1].Value = model.HeaderInfo.Client;
                if (!string.IsNullOrWhiteSpace(model.HeaderInfo.ClientLocation))
                    sheet.Cells[13, 1].Value += " - " + model.HeaderInfo.ClientLocation;

                if (!string.IsNullOrEmpty(model.HeaderInfo.NameForReceipts))
                    sheet.Cells[14, 1].Value = "Attn: " + model.HeaderInfo.NameForReceipts;

                sheet.Cells[15, 1].Value = model.HeaderInfo.AddressLine1;
                sheet.Cells[16, 1].Value = model.HeaderInfo.AddressLine2;
                sheet.Cells[17, 1].Value = model.HeaderInfo.AddressLine3;

                sheet.Cells[12, 5].Value = model.HeaderInfo.InvoiceNumber;
                sheet.Cells[12, 6].Value = model.HeaderInfo.InvoiceDate;
                sheet.Cells[14, 5].Value = model.HeaderInfo.InvoiceAmount;
                sheet.Cells[14, 6].Value = model.HeaderInfo.DueDate;

                sheet.Cells[17, 5].Value = model.HeaderInfo.InvoiceAddress1;
                sheet.Cells[18, 5].Value = model.HeaderInfo.InvoiceAddress2;

                #endregion

                #region Main

                var shifts = model.DailyActivities.First().First().Shifts;
                for (int i = 0; i < shifts.Count - 1; i++)
                {
                    sheet.InsertColumn(3, 1, 4);
                    sheet.Cells[21, 4, 24, 4].Copy(sheet.Cells[21, 3, 24, 3]);
                }

                for (int i = 0; i < shifts.Count; i++)
                {
                    sheet.Cells[23, 3 + i].Value = shifts[i].ShiftName;
                }

                for (int i = 0; i < model.DailyActivities.Count - 1; i++)
                {
                    sheet.Cells[21, 1, 25, 5 + shifts.Count].Copy(sheet.Cells[27 + i * 6, 1]);
                }

                var tableIndex = 0;
                var totalRows = new List<int>();
                foreach (var activityGroup in model.DailyActivities)
                {
                    sheet.Cells[21 + tableIndex, 1].Value = model.Invoice.DepartmentName + ", " + activityGroup.First().ActivityType;

                    var row = 24 + tableIndex;
                    foreach (var day in activityGroup)
                    {
                        sheet.InsertRow(row, 1, row + 1);
                        sheet.Cells[row, 1].Value = day.Date.DayOfWeek;
                        sheet.Cells[row, 2].Value = day.Date;

                        var columnIndex = 3;
                        foreach (var shift in day.Shifts)
                        {
                            sheet.Cells[row, columnIndex].Value = shift.NumberOfUnits;
                            columnIndex++;
                        }
                        sheet.Cells[row, columnIndex].Value = day.TotalUnits;
                        sheet.Cells[row, columnIndex + 1].Value = day.Rate;
                        sheet.Cells[row, columnIndex + 2].Value = day.TotalAmount;
                        row++;
                    }

                    sheet.Cells[row, 1, row, 5 + shifts.Count].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.None);
                    sheet.Cells[row + 1, 3 + shifts.Count].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", activityGroup.Count + 1);
                    sheet.Cells[row + 1, 5 + shifts.Count].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", activityGroup.Count + 1);
                    totalRows.Add(row + 1);

                    tableIndex += 6 + activityGroup.Count;
                }

                sheet.Cells[21 + tableIndex, 3 + shifts.Count].Value = "Grand Total:";
                sheet.Cells[21 + tableIndex, 5 + shifts.Count].Formula =
                    string.Join("+", totalRows.Select(s => string.Format("{0}{1}", string.Concat(sheet.Cells[21 + tableIndex, 5 + shifts.Count].Address.TakeWhile(char.IsLetter)), s)));
                sheet.Cells[21 + tableIndex, 5 + shifts.Count].Style.Numberformat.Format = @"\$##,##0.00";
                sheet.Cells[21 + tableIndex, 3 + shifts.Count, 21 + tableIndex, 5 + shifts.Count].Style.Font.Bold = true;

                sheet.Cells[22 + tableIndex, 3 + shifts.Count].Value = "Payment Service Charge:";
                sheet.Cells[22 + tableIndex, 5 + shifts.Count].Value = model.TotalPaymentServiceCharge;
                sheet.Cells[22 + tableIndex, 5 + shifts.Count].Style.Numberformat.Format = @"\$##,##0.00";
                sheet.Cells[22 + tableIndex, 3 + shifts.Count, 22 + tableIndex, 5 + shifts.Count].Style.Font.Bold = true;

                #endregion

                #region WorkOrders

                var woSheet = package.Workbook.Worksheets[2];
                package.Workbook.Worksheets.Delete(woSheet);

                #endregion

                return package.GetAsByteArray();
            }
        }

        public byte[] GenerateActivityInvoiceReport(ActivityInvoiceModel model)
        {
            var templatePath = ConfigurationManager.AppSettings["ActivityInvoiceTemplatePath"];
            var template = new FileInfo(templatePath);
            using (var package = new ExcelPackage(template, true))
            {
                var sheet = package.Workbook.Worksheets[1];

                #region Header

                sheet.Cells[13, 1].Value = model.HeaderInfo.Client;
                if (!string.IsNullOrWhiteSpace(model.HeaderInfo.ClientLocation))
                    sheet.Cells[13, 1].Value += " - " + model.HeaderInfo.ClientLocation;

                if (!string.IsNullOrEmpty(model.HeaderInfo.NameForReceipts))
                    sheet.Cells[14, 1].Value = "Attn: " + model.HeaderInfo.NameForReceipts;

                sheet.Cells[15, 1].Value = model.HeaderInfo.AddressLine1;
                sheet.Cells[16, 1].Value = model.HeaderInfo.AddressLine2;
                sheet.Cells[17, 1].Value = model.HeaderInfo.AddressLine3;

                sheet.Cells[12, 7].Value = model.HeaderInfo.InvoiceNumber;
                sheet.Cells[12, 8].Value = model.HeaderInfo.InvoiceDate;
                sheet.Cells[14, 7].Value = model.HeaderInfo.InvoiceAmount;
                sheet.Cells[14, 8].Value = model.HeaderInfo.DueDate;

                sheet.Cells[17, 7].Value = model.HeaderInfo.InvoiceAddress1;
                sheet.Cells[18, 7].Value = model.HeaderInfo.InvoiceAddress2;

                #endregion

                #region Main

                sheet.Cells[21, 1].Value = model.ActivityName;

                const int startRow = 24;
                var row = startRow;
                foreach (var workOrder in model.WorkOrders)
                {
                    var purchaseOrders = workOrder.PurchaseOrders;
                    if (purchaseOrders.Any() && purchaseOrders.Sum(s => s.Amount) > 0m)
                    {
                        for (int i = 0; i < purchaseOrders.Count; i++)
                        {
                            sheet.InsertRow(row, 1, row + 1);
                            if (i == 0)
                            {
                                sheet.Cells[row, 1].Value = workOrder.Date;
                                sheet.Cells[row, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                sheet.Cells[row, 2].Value = workOrder.ShiftName;
                                sheet.Cells[row, 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                sheet.Cells[row, 3].Value = workOrder.WorkOrderNumber;
                                sheet.Cells[row, 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                                sheet.Cells[row, 7].Value = workOrder.Rate;
                                sheet.Cells[row, 7].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }
                            sheet.Cells[row, 4].Value = purchaseOrders[i].NumberOfCases;
                            sheet.Cells[row, 5].Value = purchaseOrders[i].Number;
                            sheet.Cells[row, 6].Value = purchaseOrders[i].Quantity;
                            sheet.Cells[row, 8].Value = purchaseOrders[i].Amount;
                            row++;
                        }
                        if (purchaseOrders.Count > 1)
                        {
                            sheet.Cells[row - purchaseOrders.Count, 1, row - 1, 1].Merge = true;
                            sheet.Cells[row - purchaseOrders.Count, 2, row - 1, 2].Merge = true;
                            sheet.Cells[row - purchaseOrders.Count, 3, row - 1, 3].Merge = true;
                            sheet.Cells[row - purchaseOrders.Count, 7, row - 1, 7].Merge = true;
                        }
                    }
                    else
                    {
                        sheet.InsertRow(row, 1, row + 1);
                        sheet.Cells[row, 1].Value = workOrder.Date;
                        sheet.Cells[row, 2].Value = workOrder.ShiftName;
                        sheet.Cells[row, 3].Value = workOrder.WorkOrderNumber;
                        sheet.Cells[row, 4].Value = workOrder.NumberOfCases;
                        sheet.Cells[row, 5].Value = workOrder.PurchaseOrderNumber;
                        sheet.Cells[row, 6].Value = workOrder.Quantity;
                        sheet.Cells[row, 7].Value = workOrder.Rate;
                        sheet.Cells[row, 8].Value = workOrder.TotalAmount;
                        row++;
                    }
                }

                sheet.Cells[row, 1, row, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.None);
                row++;

                var rowDiff = row - startRow;
                sheet.Cells[row, 6].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", rowDiff);
                sheet.Cells[row, 8].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", rowDiff);

                #endregion

                return package.GetAsByteArray();
            }
        }

        public byte[] GenerateSelectionInvoiceReport(SelectionInvoiceModel model)
        {
            var templatePath = ConfigurationManager.AppSettings["SelectionInvoiceTemplatePath"];
            var template = new FileInfo(templatePath);
            using (var package = new ExcelPackage(template, true))
            {
                var sheet = package.Workbook.Worksheets[1];

                #region Header

                sheet.Cells[13, 1].Value = model.HeaderInfo.Client;
                if (!string.IsNullOrWhiteSpace(model.HeaderInfo.ClientLocation))
                    sheet.Cells[13, 1].Value += " - " + model.HeaderInfo.ClientLocation;

                if (!string.IsNullOrEmpty(model.HeaderInfo.NameForReceipts))
                    sheet.Cells[14, 1].Value = "Attn: " + model.HeaderInfo.NameForReceipts;

                sheet.Cells[15, 1].Value = model.HeaderInfo.AddressLine1;
                sheet.Cells[16, 1].Value = model.HeaderInfo.AddressLine2;
                sheet.Cells[17, 1].Value = model.HeaderInfo.AddressLine3;

                sheet.Cells[12, 8].Value = model.HeaderInfo.InvoiceNumber;
                sheet.Cells[12, 9].Value = model.HeaderInfo.InvoiceDate;
                sheet.Cells[14, 8].Value = model.HeaderInfo.InvoiceAmount;
                sheet.Cells[14, 9].Value = model.HeaderInfo.DueDate;

                sheet.Cells[17, 8].Value = model.HeaderInfo.InvoiceAddress1;
                sheet.Cells[18, 8].Value = model.HeaderInfo.InvoiceAddress2;

                #endregion

                #region Main

                var row = 24;
                if (model.DefaultWorkers.Any())
                {
                    if (model.Associates.Any())
                    {
                        row = ExportAssociates(model.Associates, sheet, row);
                        row++;
                    }
                    row = ExportManagers(model.Managers, sheet, row);
                    ExportTotals(model.DefaultWorkers.Count + 2, sheet, row);
                    row = 30 + model.DefaultWorkers.Count + 1;
                }
                else
                {
                    var defaultHeaderRow = 24 - 4;
                    sheet.DeleteRow(defaultHeaderRow, 7);
                    row = 24;
                }

                if (model.TrainerWorkers.Any())
                {
                    if (model.TrainerAssociates.Any())
                    {
                        row = ExportAssociates(model.TrainerAssociates, sheet, row);
                        row++;
                    }
                    row = ExportManagers(model.TrainerManagers, sheet, row);
                    ExportTotals(model.TrainerWorkers.Count + 2, sheet, row);
                }
                else
                {
                    var trainerHeaderRow = 30 + model.DefaultWorkers.Count + 1;
                    trainerHeaderRow -= 3;
                    sheet.DeleteRow(trainerHeaderRow, 5);
                }

                #endregion

                return package.GetAsByteArray();
            }
        }

        private static void ExportTotals(int workers, ExcelWorksheet sheet, int row)
        {
            sheet.Cells[row, 1, row, 9].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.None);
            sheet.Cells[row + 1, 3].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", workers);
            sheet.Cells[row + 1, 5].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", workers);
            sheet.Cells[row + 1, 7].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", workers);
            sheet.Cells[row + 1, 9].FormulaR1C1 = string.Format("SUM(R[-{0}]C:R[-2]C)", workers);
        }

        private static int ExportManagers(List<SelectionInvoiceWorker> managers, ExcelWorksheet sheet, int row)
        {
            var first = true;
            foreach (var w in managers)
            {
                sheet.InsertRow(row, 1, row + 1);
                if (first)
                {
                    sheet.Cells[row, 1].Value = "Managers";
                    first = false;
                }
                sheet.Cells[row, 2].Value = w.LastName + ", " + w.FirstName + " (" + w.WorkerType + ")";
                sheet.Cells[row, 3].Value = w.RegularHours;
                sheet.Cells[row, 4].Value = w.RegularRate;
                sheet.Cells[row, 5].Value = w.OvertimeHours;
                sheet.Cells[row, 6].Value = w.OvertimeRate;
                sheet.Cells[row, 7].Value = w.DoubleOvertimeHours;
                sheet.Cells[row, 8].Value = w.DoubleOvertimeRate;
                sheet.Cells[row, 9].FormulaR1C1 = "RC[-6]*RC[-5]+RC[-4]*RC[-3]+RC[-2]*RC[-1]";
                row++;
            }
            return row;
        }

        private static int ExportAssociates(List<SelectionInvoiceWorker> associates, ExcelWorksheet sheet, int row)
        {
            var first = true;
            foreach (var w in associates)
            {
                sheet.InsertRow(row, 1, row + 1);
                if (first)
                {
                    sheet.Cells[row, 1].Value = "Associates";
                    first = false;
                }
                sheet.Cells[row, 2].Value = w.LastName + ", " + w.FirstName;
                sheet.Cells[row, 3].Value = w.RegularHours;
                sheet.Cells[row, 4].Value = w.RegularRate;
                sheet.Cells[row, 5].Value = w.OvertimeHours;
                sheet.Cells[row, 6].Value = w.OvertimeRate;
                sheet.Cells[row, 7].Value = w.DoubleOvertimeHours;
                sheet.Cells[row, 8].Value = w.DoubleOvertimeRate;
                sheet.Cells[row, 9].FormulaR1C1 = "RC[-6]*RC[-5]+RC[-4]*RC[-3]+RC[-2]*RC[-1]";
                row++;
            }

            sheet.InsertRow(row, 1, row + 1);
            return row;
        }

        private Image getImage(string url)
        {
            using (var client = new WebClient())
            {
                var bytes = client.DownloadData(url);
                using (var stream = new MemoryStream(bytes))
                    return Image.FromStream(stream);
            }
        }

        private ExcelPicture putImageToWorksheet(ExcelWorksheet sheet, Image image, int row, int column, int? height = null)
        {
            if (!height.HasValue) height = image.Height;
            var aspectRatio = (double)image.Width / image.Height;
            var excelPicture = sheet.Drawings.AddPicture(Path.GetRandomFileName(), image);
            excelPicture.SetSize((int)(height * aspectRatio), height.Value);
            excelPicture.SetPosition(row, 0, column, 0);
            return excelPicture;
        }
    }
}