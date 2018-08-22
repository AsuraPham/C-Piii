using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Merit.Components.Invoices.Domain.ReportModel.Unloading;
using OfficeOpenXml;
using WebGrease.Css.Extensions;
using OfficeOpenXml.Style;

namespace Merit.Invoicing.Excel
{
    public class KrogerExcelGenerator
    {
        public string TemplatePath = "C:/Surge/Kiln/MeritWeb/src/Merit.Invoicing/ExcelTemplates/KrogerInvoiceTemplate.xlsx";

        public const string OutputDirectory = "Output/";

        public const string OutputFileName = "KrogerInvoice-{0}.xlsx";

        public const string CurrencyFormatCode = "[$$-409]#,##0.00;-[$$-409]#,##0.00";

        public const string EmphasizeFillColor = "#FF9900";

        public KrogerExcelGenerator(string templatePath)
        {
            TemplatePath = templatePath;
        }

        public byte[] Generate(KrogerUnloadingInvoiceModel invoice)
        {
            var template = new FileInfo(TemplatePath);

            using (var package = new ExcelPackage(template, true))
            {
                var sheets = package.Workbook.Worksheets;

                var blankUnloadActivitySheet = sheets.First(w => w.Name == "Blank Unload Activity");
                createUnloadActivityWorksheets(invoice, sheets, blankUnloadActivitySheet);
                createSummaryWorksheet(invoice, sheets);
                createMainInvoiceWorksheet(invoice, sheets);

                return package.GetAsByteArray();
            }
        }

        private void createMainInvoiceWorksheet(KrogerUnloadingInvoiceModel invoice, ExcelWorksheets sheets)
        {
            var sheet = sheets.First(w => w.Name == "Invoice");

            var allOrders = invoice.AllWorkOrders;

            var curRow = 21;
            var subtotalRows = new List<int>();
            if (invoice.BreakdownByDepartment)
            {
                // Create sub tables for each department
                foreach (var group in invoice.AllWorkOrders.GroupBy(i => new { i.LocationDepartmentId, i.DepartmentName }))
                {
                    curRow = buildMainInvoiceSubTable(invoice, sheet, curRow, group, group.Key.DepartmentName);
                    subtotalRows.Add(curRow);
                    curRow += 2;
                }
            } 
            else
            {
                curRow = buildMainInvoiceSubTable(invoice, sheet, curRow, invoice.AllWorkOrders);
                subtotalRows.Add(curRow);
            }

            sheet.SetValue(13, 1, String.Format("{0} - {1}", invoice.HeaderInfo.Client, invoice.HeaderInfo.ClientLocation));
            sheet.SetValue(14, 1, "Attn: " + invoice.HeaderInfo.NameForReceipts);
            sheet.SetValue(15, 1, invoice.HeaderInfo.AddressLine1);
            sheet.SetValue(16, 1, invoice.HeaderInfo.AddressLine2);
            sheet.SetValue(17, 1, invoice.HeaderInfo.AddressLine3);

            sheet.SetValue(12, 6, invoice.HeaderInfo.InvoiceNumber);
            sheet.SetValue(12, 7, invoice.HeaderInfo.InvoiceDate.Value.ToString("MM/dd/yyyy"));
            // Amount Due is calculated as sum of all subtotals
            sheet.Cells[14, 6].Formula = String.Join("+", subtotalRows.Select(row => String.Format("G{0}", row)));
            sheet.SetValue(14, 7, invoice.HeaderInfo.InvoiceTerms);

            sheet.Cells[17, 6].Value = invoice.HeaderInfo.InvoiceAddress1;
            sheet.Cells[18, 6].Value = invoice.HeaderInfo.InvoiceAddress2;

            sheet.Calculate();
            sheet.Select();
        }

        private int buildMainInvoiceSubTable(KrogerUnloadingInvoiceModel invoice, ExcelWorksheet sheet, int startRow, IEnumerable<UnloadingWorkOrderModel> workOrders, string name = null)
        {
            // header
            if (startRow > 23)
            {
                sheet.Cells[21, 1, 23, 7].Copy(sheet.Cells[startRow, 1, startRow + 2, 7]);
            }
            if (name != null)
            {
                sheet.Cells[startRow, 1].Value = name;
            }

            startRow += 3;

            var weekBeginDate = (invoice.HeaderInfo.ServiceDateStart.HasValue)
                        ? invoice.HeaderInfo.ServiceDateStart.Value.ToShortDateString()
                        : "";
            var weekEndDate = (invoice.HeaderInfo.ServiceDateEnd.HasValue)
                        ? invoice.HeaderInfo.ServiceDateEnd.Value.ToShortDateString()
                        : "";

            var clientOrders = workOrdersChargedToClient(workOrders, invoice);
            var carrierOrders = workOrdersChargedToCarrier(workOrders, invoice);

            var loadLines = workOrdersByReferenceCode(clientOrders)
                .Select(l => new MainInvoiceLoadItem
                {
                    Location = String.Format("{0}, {1}", invoice.HeaderInfo.Client, invoice.HeaderInfo.LocationShortAddress),
                    BillTo = l.Key,
                    WeekStart = weekBeginDate,
                    WeekEnd = weekEndDate,
                    Amount = (Double)l.Sum(w => w.TotalAmount),
                })
                .OrderBy(i => i.BillTo)
                .ToList();

            var rebateLoadLines = workOrdersByReferenceCode(carrierOrders)
                .Select(l => new MainInvoiceLoadItem
                {
                    Location = String.Format("{0}, {1}", invoice.HeaderInfo.Client, invoice.HeaderInfo.LocationShortAddress),
                    BillTo = String.Format("{0} Rebate", l.Key),
                    WeekStart = weekBeginDate,
                    WeekEnd = weekEndDate,
                    Amount = -(Double)l.Sum(w => w.Rebate),
                })
                .OrderBy(i => i.BillTo)
                .ToList();

            var allLoadLines = loadLines.Concat(rebateLoadLines).ToList();
            var currentRow = startRow;
            foreach (var item in allLoadLines)
            {
                addMainInvoiceLine(sheet, currentRow, item);
                ++currentRow;
            }

            // Create Invoice Total Formula:
            var invoiceLinesTotalRow = startRow + allLoadLines.Count + 1;

            sheet.Cells[invoiceLinesTotalRow, 6].Value = "Subtotal:";
            sheet.Cells[invoiceLinesTotalRow, 7].Formula = String.Format("SUM(G{0}:G{1})", startRow, invoiceLinesTotalRow - 2);
            sheet.Cells[invoiceLinesTotalRow, 7].Style.Numberformat.Format = CurrencyFormatCode;

            return invoiceLinesTotalRow;
        }
        
        private IEnumerable<UnloadingWorkOrderModel> workOrdersChargedToClient(IEnumerable<UnloadingWorkOrderModel> workOrders, KrogerUnloadingInvoiceModel invoice)
        {
            return workOrders.Where(i => i.BillTo == invoice.HeaderInfo.Client);
        }

        private IEnumerable<UnloadingWorkOrderModel> workOrdersChargedToCarrier(IEnumerable<UnloadingWorkOrderModel> workOrders, KrogerUnloadingInvoiceModel invoice)
        {
            return workOrders.Where(i => i.BillTo != invoice.HeaderInfo.Client);
        }

        private IEnumerable<IGrouping<string, UnloadingWorkOrderModel>> workOrdersByReferenceCode(IEnumerable<UnloadingWorkOrderModel> workOrders)
        {
            return workOrders
                    .OrderBy(o => o.LoadDate)
                    .GroupBy(o => o.ReferenceNumber);
        }

        private void addMainInvoiceLine(ExcelWorksheet sheet, int row, MainInvoiceLoadItem data)
        {
            sheet.InsertRow(row, 1);

            var valuesInOrder = new Object[]
                {
                    data.Location,
                    data.BillTo,
                    data.WeekStart,
                    data.WeekEnd,
                    data.InvoiceNum,
                    data.RebateCreditNum,
                    data.Amount
                };

            SetRowValues(sheet, row, 1, valuesInOrder);
            SetBackgroundColor(sheet.Cells[row, 3, row, 6], EmphasizeFillColor);
            sheet.Cells[row, 1, row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[row, 7].Style.Numberformat.Format = CurrencyFormatCode; // give currency format to money cells
        }

        private void createSummaryWorksheet(KrogerUnloadingInvoiceModel invoice, ExcelWorksheets sheets)
        {
            var summarySheet = sheets.First(w => w.Name == "Summary");
            summarySheet.Select();

            if (invoice.BreakdownByDepartment)
            {
                var curRow = 2;
                foreach (var group in invoice.AllWorkOrders.GroupBy(i => new { i.LocationDepartmentId, i.DepartmentName }))
                {
                    curRow = buildSummarySubTable(invoice, summarySheet, curRow, group, group.Key.DepartmentName) + 3;
                }
            }
            else
            {
                buildSummarySubTable(invoice, summarySheet, 2, invoice.AllWorkOrders);
            }
        }

        private int buildSummarySubTable(KrogerUnloadingInvoiceModel invoice, ExcelWorksheet sheet, int startRow, IEnumerable<UnloadingWorkOrderModel> workOrders, string name = null)
        {
            // header
            if (startRow > 3)
            {
                sheet.Cells[2, 2, 2, 5].Copy(sheet.Cells[startRow, 2, startRow, 5]);
                addSummaryLine(sheet);
            }
            if (name != null)
            {
                sheet.Cells[startRow, 2].Value = String.Format("Kroger {0} Activity", name);
            }

            sheet.SetValue(startRow, 5, (invoice.HeaderInfo.ServiceDateEnd.HasValue)
                        ? invoice.HeaderInfo.ServiceDateEnd.Value.ToString("MM/dd/yyyy")
                        : string.Empty);

            var loadsChargedToClient = workOrdersChargedToClient(workOrders, invoice);
            var loadsChargedToCarrier = workOrdersChargedToCarrier(workOrders, invoice);

            var totalLoadsLines = new List<SummaryLineItem>
            {
                new SummaryLineItem
                {
                    Name = "TOTAL LOADS",
                },
                new SummaryLineItem
                {
                    Name = "# OF LOADS CHARGED TO CLIENT",
                    Value = loadsChargedToClient.Count(),
                    EmphasizeValue = true
                },
                new SummaryLineItem
                {
                    Name = "% OF LOADS CHARGED TO CLIENT",
                    IsFormula = true,
                    NumberFormat = "0.0%",
                    Value = string.Format("C{0}/C{1}", startRow + 3, startRow + 2)
                },
                new SummaryLineItem
                {
                    Name = "# OF LOADS CHARGED TO CARRIER",
                    Value = loadsChargedToCarrier.Count(),
                    EmphasizeValue = true
                },
                new SummaryLineItem
                {
                    Name = "% OF LOADS CHARGED TO CARRIER",
                    IsFormula = true,
                    NumberFormat = "0.0%",
                    Value = string.Format("C{0}/C{1}", startRow + 5, startRow + 2)
                },
            };

            totalLoadsLines.ForEach(l => addSummaryLine(sheet, l));
            addSummaryLine(sheet);

            sheet.Cells[startRow + 2, 3].Formula = string.Format("C{0}+C{1}", startRow + 3, startRow + 5);

            var unloadChargesLines = workOrdersByReferenceCode(loadsChargedToClient)
                .Select(gr => new SummaryLineItem
                {
                    Name = String.Format("Unload Charges - {0}", gr.Key).ToUpper(),
                    Value = gr.Sum(w => w.TotalAmount),
                    NumberFormat = CurrencyFormatCode,
                    EmphasizeValue = true
                })
                .ToList();

            var rebateAmountLines = workOrdersByReferenceCode(loadsChargedToCarrier)
                .Select(gr => new SummaryLineItem
                {
                    Name = String.Format("Rebate Amt - {0}", gr.Key).ToUpper(),
                    Value = -gr.Sum(w => w.Rebate),
                    NumberFormat = CurrencyFormatCode,
                    EmphasizeValue = true
                })
                .ToList();

            var unloadChargesRowNums = new List<int>();
            var rebateAmountRowNums = new List<int>();
            unloadChargesLines.ForEach(l =>
            {
                unloadChargesRowNums.Add(addSummaryLine(sheet, l));
                addSummaryLine(sheet);
            });
            rebateAmountLines.ForEach(l =>
            {
                rebateAmountRowNums.Add(addSummaryLine(sheet, l));
                addSummaryLine(sheet);
            });
            addSummaryLine(sheet);

            int totalRowFirst = addSummaryLine(sheet, new SummaryLineItem
            {
                Name = "TOTAL REBATE AMT",
                IsFormula = true,
                Value = rebateAmountRowNums.Count > 0
                        ? String.Join("+", rebateAmountRowNums.Select(s => String.Format("C{0}", s)))
                        : "0",
                NumberFormat = CurrencyFormatCode,
            });

            addSummaryLine(sheet, new SummaryLineItem
            {
                Name = "TOTAL UNLOAD CHARGES",
                IsFormula = true,
                Value = unloadChargesRowNums.Count > 0
                        ? String.Join("+", unloadChargesRowNums.Select(s => String.Format("C{0}", s)))
                        : "0",
                NumberFormat = CurrencyFormatCode,
            });

            addSummaryLine(sheet, new SummaryLineItem
            {
                Name = "NET REBATE AMT",
                Value = 0.00, // manually entered by user in Excel
                NumberFormat = CurrencyFormatCode,
            });

            int lastRowNum = addSummaryLine(sheet, new SummaryLineItem
            {
                Name = "NET AMOUNT DUE",
                IsFormula = true,
                Value = String.Format("SUM(C{0}:C{1})", totalRowFirst, totalRowFirst + 2),
                NumberFormat = CurrencyFormatCode,
                EmphasizeName = true,
                EmphasizeValue = true,
            });

            sheet.Cells[startRow + 2, 2, lastRowNum, 3].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[startRow + 2, 5, lastRowNum, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            return lastRowNum;
        }

        private int addSummaryLine(ExcelWorksheet summarySheet, SummaryLineItem lineItem = null)
        {
            var newRowNum = summarySheet.Dimension != null ? summarySheet.Dimension.End.Row + 1 : 1;
            if (newRowNum < 4)
            {
                newRowNum = 4;
            }
            summarySheet.InsertRow(newRowNum, 1);

            if (lineItem == null)
            {
                summarySheet.SetValue(newRowNum, 1, " "); // Blank Line:
                return newRowNum;
            }

            summarySheet.SetValue(newRowNum, 2, lineItem.Name);
            if (lineItem.IsFormula)
            {
                summarySheet.Cells[newRowNum, 3].Formula = lineItem.Value.ToString();
            }
            else
            {
                summarySheet.SetValue(newRowNum, 3, lineItem.Value);
            }
            summarySheet.SetValue(newRowNum, 5, lineItem.InvoiceNumber);

            if (lineItem.NumberFormat != String.Empty)
            {
                summarySheet.Cells[newRowNum, 3].Style.Numberformat.Format = lineItem.NumberFormat;
            }
            if (lineItem.EmphasizeName)
            {
                SetBackgroundColor(summarySheet.Cells[newRowNum, 2], EmphasizeFillColor);
            }
            if (lineItem.EmphasizeValue)
            {
                SetBackgroundColor(summarySheet.Cells[newRowNum, 3], EmphasizeFillColor);
            }
            summarySheet.Cells[newRowNum, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            return newRowNum;
        }

        private void createUnloadActivityWorksheets(KrogerUnloadingInvoiceModel invoice, ExcelWorksheets sheets, ExcelWorksheet blankUnloadActivitySheet)
        {
            // Create and populate "All Loads" worksheet:
            var allLoadsSheet = sheets.Add("All Loads", blankUnloadActivitySheet);
            populateUnloadActivitySheet(allLoadsSheet, invoice.AllWorkOrders);

            // Create and populate "Street" worksheet:
            var streetSheet = sheets.Add("Street", blankUnloadActivitySheet);
            populateUnloadActivitySheet(streetSheet, invoice.PrepaidWorkOrders);

            // Create and populate "Controlled" worksheet:
            var controlledSheet = sheets.Add("Controlled", blankUnloadActivitySheet);
            populateUnloadActivitySheet(controlledSheet, invoice.ControlledWorkOrders);

            // Get the names of all unique department #'s in the collection of work orders:
            var controlledWorkOrdersByRefNo = invoice.ControlledWorkOrdersByReferenceCode;
            var allRefNames = controlledWorkOrdersByRefNo
                .Select(g => g[0].ReferenceNumber)
                .Where(s => !String.IsNullOrEmpty(s))
                .ToList();

            // Create workSheets for each unique Ref in the collection of invoices:
            allRefNames.ForEach(n =>
            {
                var sheet = sheets.Add(String.Format("Controlled ({0})", n), blankUnloadActivitySheet);

                var workOrdersFromThisRef = controlledWorkOrdersByRefNo.Find(l => l[0].ReferenceNumber == n);
                populateUnloadActivitySheet(sheet, workOrdersFromThisRef);
            });

            if (invoice.BreakdownByDepartment)
            {
                // Create workSheets for each department
                foreach (var gr in invoice.AllWorkOrders.GroupBy(x => new { x.LocationDepartmentId, x.DepartmentName }))
                {
                    var departmentSheet = sheets.Add(gr.Key.DepartmentName, blankUnloadActivitySheet);
                    populateUnloadActivitySheet(departmentSheet, gr.ToList());
                }
            }

            sheets.Delete(blankUnloadActivitySheet);
        }

        private void populateUnloadActivitySheet(ExcelWorksheet worksheet, IList<UnloadingWorkOrderModel> workOrders)
        {
            worksheet.Select();
            workOrders.ForEach(w => addWorkOrderLine(worksheet, w));
            worksheet.Cells[5, 12, workOrders.Count + 7, 17].Style.Numberformat.Format = CurrencyFormatCode;
            var lastRowNo = worksheet.Dimension.Rows;
            worksheet.SetValue(lastRowNo, 11, "Total");
            var columns = "LMNOPQ";
            for (int i = 0; i < columns.Length; i++)
            {
                worksheet.Cells[lastRowNo, 12 + i].Formula = String.Format("SUM({0}5:{0}{1})", columns[i], (workOrders.Count + 4));
            }
        }

        private static void addWorkOrderLine(ExcelWorksheet worksheet, UnloadingWorkOrderModel workOrder)
        {
            //var newRowNo = worksheet.Dimension.Rows + 1;
            var newRowNo = worksheet.Dimension.Rows;
            worksheet.InsertRow(newRowNo, 1);
            var rowData = new Object[]
            {
                workOrder.LoadDate.ToShortDateString(),
                workOrder.Service,
                workOrder.WorkOrderNumber,
                workOrder.DepartmentCode,
                workOrder.ReferenceNumber,
                workOrder.TrailerNumber,
                workOrder.PurchaseOrderNumber,
                workOrder.Carrier,
                workOrder.Vendor,
                workOrder.BillTo,
                workOrder.PayType,
                workOrder.Rate,
                workOrder.RestackFee,
                workOrder.AdditionalCharges,
                workOrder.ProcessFees,
                workOrder.TotalAmount,
                workOrder.Rebate,
            };
            SetRowValues(worksheet, newRowNo, 1, rowData);
        }
        
        /*private static void CopyRange(ExcelWorksheet worksheet, int srcRowNum, int srcColFrom, int dstRowNum, int dstColFrom, int count)
        {
            for (int i = 0; i < count; i++)
            {
                worksheet.Cells[srcRowNum, srcColFrom, srcRowNum, srcColFrom + count].Copy(worksheet.Cells[dstRowNum, dstColFrom, dstRowNum, dstColFrom + count]);
                var src = worksheet.Cells[srcRowNum, srcColFrom + i];
                var dst = worksheet.Cells[dstRowNum, dstColFrom + i];
                dst.Value = src.Value;
                dst.Style.Border.
            }
        }*/

        private static void SetRowValues(ExcelWorksheet worksheet, int rowNum, int colFrom, Object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                worksheet.SetValue(rowNum, i + colFrom, data[i]);
            }
        }

        private static void SetBackgroundColor(ExcelRange range, string color)
        {
            var fill = range.Style.Fill;
            fill.PatternType = ExcelFillStyle.Solid;
            fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml(color));
        }
    }

    internal class SummaryLineItem
    {
        public String Name { get; set; }
        public object Value { get; set; }
        public String InvoiceNumber { get; set; }
        public String NumberFormat { get; set; }
        public bool IsFormula { get; set; }
        public bool EmphasizeName { get; set; }
        public bool EmphasizeValue { get; set; }
    }

    internal class MainInvoiceLoadItem
    {
        public String Location { get; set; }
        public String BillTo { get; set; }
        public String WeekStart { get; set; }
        public String WeekEnd { get; set; }
        public String InvoiceNum { get; set; }
        public String RebateCreditNum { get; set; }
        public Double Amount { get; set; }
    }
}