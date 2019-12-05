using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Excels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Resources;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using OfficeOpenXml;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fanex.Athena.Report.ByPlayer.Excels
{
    public class CustomerWinLossExcel : BaseExcel, ICustomerWinLossExcel
    {

        private const int TemplateSheetIndex = 0;

        private const int TemplateTitleRowIndex = 1;
        private const int StartTemplateHeaderRowIndex = 2;
        private const int EndTemplateHeaderRowIndex = 3;
        private const int TemplateDataRowIndex = 4;
        private const int TemplateTotalRowIndex = 5;
        private const int TemplateNoInfoRowIndex = 6;

        public const string FormatDate = "MM/dd/yyyy";
        private const string SheetName = "Data";

        private const int ColumnIndex = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnTurnover = 4;
        private const int ColumnTotalTransaction = 5;

        private const int ColumnCustomerWl = 6;
        private const int ColumnCustomerComm = 7;
        private const int ColumnCustomerTotal = 8;

        private const int ColumnBetEffective = 9;
        private const int ColumnLicenseeWl = 10;
        private const int ColumnBaCommission = 11;
        private const int ColumnAmtDueToBa = 12;

        private const int ColumnTemplateCount = 12;

        private readonly string[] HeaderTextList = new[]
            {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.EffectiveBettingValue,
                ViewResources.LicenseeW_L,
                ViewResources.BACommission,
                ViewResources.AmtDueToBA
            };

        public CustomerWinLossExcel(IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment) : base(enviroment, fileService, excelOptionsAccessor)
        {
        }

        public string Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            return CreateFileStream(template, data, metaModel);
        }


        private string CreateFileStream(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            var fileInfo = new FileInfo(template);
            using (var package = new ExcelPackage(fileInfo, false))
            {
                var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
                var title = BuildExcelTitle(metaModel);

                if (data.DataList.Any())
                {
                    var totalRows = data.DataList.Count();
                    var numberOfSheet = ExcelHelper.GetNumberOfSheets(totalRows, ExcelOptions.MaximumRowPerFile);

                    for (int sheetIndex = 0; sheetIndex < numberOfSheet; sheetIndex++)
                    {
                        var currentSheet = package.Workbook.Worksheets.Add($"{SheetName}{(sheetIndex + 1)}", templateSheet);
                        currentSheet.DeleteRow(TemplateNoInfoRowIndex, 1);

                        AssignTitle(currentSheet, title);
                        AssignHeader(currentSheet);
                        AssignSumData(currentSheet, data.TotalData);
                        AssignDataList(currentSheet, sheetIndex, ExcelOptions.MaximumRowPerFile, totalRows, data.DataList);

                        ProcessUI(currentSheet, metaModel);
                    }
                }
                else
                {
                    var sheet = package.Workbook.Worksheets.Add(SheetName, templateSheet);
                    AssignTitle(sheet, title);
                    AssignHeader(sheet);
                    AssignNoInfoRow(sheet);
                    ProcessUI(sheet, metaModel);

                    const byte DeletedRowCount = 2;
                    sheet.DeleteRow(TemplateDataRowIndex, DeletedRowCount, true);
                }

                package.Workbook.Worksheets.Delete(TemplateSheetIndex);
                var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");
                using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    package.SaveAs(stream);
                }
                return fileName;
            }

        }

        private static void AssignTitle(ExcelWorksheet sheet, string reportTitle)
        {
            var destinationCell = sheet.Cells[TemplateTitleRowIndex, ColumnIndex, TemplateTitleRowIndex, ColumnTemplateCount];
            destinationCell.Merge = true;

            sheet.SetValue(TemplateTitleRowIndex, ColumnIndex, reportTitle);
        }

        private void AssignHeader(ExcelWorksheet sheet)
        {
            for (var i = 1; i <= HeaderTextList.Length; i++)
            {
                sheet.SetValue(StartTemplateHeaderRowIndex, i, HeaderTextList[i - 1]);
            }

            var customerWLCell = sheet.Cells[StartTemplateHeaderRowIndex, ColumnCustomerWl, StartTemplateHeaderRowIndex, ColumnCustomerTotal];
            customerWLCell.Merge = true;

            sheet.SetValue(EndTemplateHeaderRowIndex, ColumnCustomerWl, $"{ViewResources.Win}/{ViewResources.Loss}");
            sheet.SetValue(EndTemplateHeaderRowIndex, ColumnCustomerComm, ViewResources.Comm);
            sheet.SetValue(EndTemplateHeaderRowIndex, ColumnCustomerTotal, ViewResources.Total);

            sheet.View.FreezePanes(TemplateDataRowIndex, ColumnIndex);
        }

        public static void AssignDataList(
            ExcelWorksheet sheet,
            int pageIndex,
            int pageSize,
            int totalRows,
            IEnumerable<CustomerWinLoss> dataList)
        {
            var startIndex = pageIndex * pageSize;
            var endIndex = Math.Min(totalRows, checked((pageIndex + 1) * pageSize));
            var itemCountPerSheet = endIndex - startIndex;

            sheet.InsertRow(TemplateDataRowIndex + 1, itemCountPerSheet - 1, TemplateDataRowIndex);

            var rowIndexPerSheet = 0;

            for (int index = startIndex; index < endIndex; index++)
            {
                var currentRow = rowIndexPerSheet + TemplateDataRowIndex;

                var customerWinloss = dataList.ElementAt(index);
                sheet.SetValue(currentRow, ColumnIndex, index + 1);
                sheet.SetValue(currentRow, ColumnExternalId, customerWinloss.ExternalId);
                sheet.SetValue(currentRow, ColumnSystemId, customerWinloss.SystemId);

                AssignCommonCells(sheet, customerWinloss, currentRow);

                rowIndexPerSheet++;
            }
        }

        private static void AssignSumData(ExcelWorksheet sheet, CustomerWinLoss totalData)
        {
            AssignCommonCells(sheet, totalData, TemplateTotalRowIndex);

            var mergedCellRange = sheet.Cells[TemplateTotalRowIndex, ColumnIndex, TemplateTotalRowIndex, ColumnSystemId];
            mergedCellRange.Value = ViewResources.Total;
            mergedCellRange.Merge = true;
        }

        private static void AssignCommonCells(ExcelWorksheet sheet, CustomerWinLoss dataItem, int rowIndex)
        {
            sheet.SetValue(rowIndex, ColumnTurnover, dataItem.Turnover);
            sheet.SetValue(rowIndex, ColumnTotalTransaction, dataItem.TotalTransaction);
            sheet.SetValue(rowIndex, ColumnCustomerWl, dataItem.CustomerWL);
            sheet.SetValue(rowIndex, ColumnCustomerComm, dataItem.PlayerComm);
            sheet.SetValue(rowIndex, ColumnCustomerTotal, dataItem.CustomerWL + dataItem.PlayerComm);
            sheet.SetValue(rowIndex, ColumnBetEffective, dataItem.BetEffective);
            sheet.SetValue(rowIndex, ColumnLicenseeWl, dataItem.LicenseeWL);
            sheet.SetValue(rowIndex, ColumnBaCommission, dataItem.BAComm);
            sheet.SetValue(rowIndex, ColumnAmtDueToBa, dataItem.AmtDueToBA);
        }

        private static string BuildExcelTitle(CustomerWinLossMetaModel metaModel)
        {
            var filterTitle = ExcelTitleBuilder.GetTitleCurrencyName(ViewResources.Viewin, metaModel.IsConvert, metaModel.BaseCurrency, metaModel.CurrencyName)
                            + ExcelTitleBuilder.GetLicenseeWLProduct(metaModel.ProductOptions, ViewResources.LicenseeW_L)
                            + (string.IsNullOrEmpty(metaModel.Username) ? string.Empty : $", {ViewResources.Username}: {metaModel.Username}");

            return string.Format(ViewResources.CustomerWinLossTitle,
                                        metaModel.FromDate.ToString(FormatDate),
                                        metaModel.ToDate.ToString(FormatDate),
                                        filterTitle);
        }

        private static void AssignNoInfoRow(ExcelWorksheet sheet)
        {
            sheet.SetValue(TemplateNoInfoRowIndex, 1, ViewResources.NoInfo);
            var noInfoRow = sheet.Cells[TemplateNoInfoRowIndex, ColumnIndex, TemplateNoInfoRowIndex, ColumnTemplateCount];
            noInfoRow.Merge = true;
        }

        private static void ProcessUI(ExcelWorksheet sheet, CustomerWinLossMetaModel metaModel)
        {
            if (!metaModel.SystemIdVisible)
            {
                sheet.Column(ColumnSystemId).Hidden = true;
            }

            if (!metaModel.HasComm)
            {
                sheet.Column(ColumnCustomerTotal).Hidden = true;
                sheet.Column(ColumnCustomerComm).Hidden = true;

                var customerWLCell = sheet.Cells[StartTemplateHeaderRowIndex, ColumnCustomerWl, StartTemplateHeaderRowIndex, ColumnCustomerTotal];
                customerWLCell.Merge = false;
                sheet.Row(EndTemplateHeaderRowIndex).Hidden = true;
            }

            if (!metaModel.IsAllowedBA)
            {
                sheet.Column(ColumnBaCommission).Hidden = true;
                sheet.Column(ColumnAmtDueToBa).Hidden = true;
            }
        }
    }
}