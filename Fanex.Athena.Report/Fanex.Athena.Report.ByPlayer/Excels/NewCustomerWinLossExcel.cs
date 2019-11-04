using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using OfficeOpenXml;

namespace Fanex.Athena.Report.ByPlayer.Excels
{
    public class NewCustomerWinLossExcel : ICustomerWinLossExcel
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

        public byte[] Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            using (var excelPackage = GetExcelPackage(template, data, metaModel))
            {
                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] ExportFile(string template, IList<ResultModel<CustomerWinLoss>> data, CustomerWinLossMetaModel metaModel)
        {
            throw new NotImplementedException();
        }

        private static ExcelPackage GetExcelPackage(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel)
        {
            const byte PageIndex = 0;
            var title = BuildExcelTitle(metaModel);

            var fi = new FileInfo(template);

            var package = new ExcelPackage(fi);
            var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
            var currentSheet = package.Workbook.Worksheets.Add(SheetName);

            AssignTitle(templateSheet, currentSheet, title);
            AssignHeader(templateSheet, currentSheet);
            AssignSheetData(templateSheet, currentSheet, PageIndex, metaModel.PageSize, data);

            ProcessUI(currentSheet, metaModel);

            package.Workbook.Worksheets.Delete(0);

            return package;
        }

        private static void AssignTitle(ExcelWorksheet templateSheet, ExcelWorksheet currentSheet, string reportTitle)
        {
            var templateCell = templateSheet.Cells[TemplateTitleRowIndex, ColumnIndex];
            var destinationCell = currentSheet.Cells[TemplateTitleRowIndex, ColumnIndex, TemplateTitleRowIndex, ColumnTemplateCount];
            templateCell.Copy(destinationCell);

            destinationCell.Merge = true;

            currentSheet.SetValue(TemplateTitleRowIndex, ColumnIndex, reportTitle);
        }

        private static void AssignHeader(ExcelWorksheet templateSheet, ExcelWorksheet currentSheet)
        {
            var headerTextList = new[]
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

            var sourceRow = templateSheet.Cells[StartTemplateHeaderRowIndex, ColumnIndex, EndTemplateHeaderRowIndex, ColumnTemplateCount];
            var destinationRow = currentSheet.Cells[StartTemplateHeaderRowIndex, ColumnIndex, EndTemplateHeaderRowIndex, ColumnTemplateCount];
            sourceRow.Copy(destinationRow);

            for (var i = 1; i <= headerTextList.Length; i++)
            {
                currentSheet.Column(i).Width = templateSheet.Column(i).Width;

                currentSheet.SetValue(StartTemplateHeaderRowIndex, i, headerTextList[i - 1]);
            }

            var customerWLCell = currentSheet.Cells[StartTemplateHeaderRowIndex, ColumnCustomerWl, StartTemplateHeaderRowIndex, ColumnCustomerTotal];
            customerWLCell.Merge = true;

            currentSheet.SetValue(EndTemplateHeaderRowIndex, ColumnCustomerWl, ViewResources.WinLoss);
            currentSheet.SetValue(EndTemplateHeaderRowIndex, ColumnCustomerComm, ViewResources.Comm);
            currentSheet.SetValue(EndTemplateHeaderRowIndex, ColumnCustomerTotal, ViewResources.Total);

            currentSheet.View.FreezePanes(TemplateDataRowIndex, ColumnTemplateCount + 1);
        }

        public static void AssignSheetData(
            ExcelWorksheet templateSheet,
            ExcelWorksheet currentSheet,
            int pageIndex,
            int pageSize,
            ResultModel<CustomerWinLoss> data)
        {
            var dataList = data.DataList;
            var rowCount = Math.Min(dataList.Count(), checked((pageIndex + 1) * pageSize));
            var rowIndex = 0;

            for (int index = pageIndex * pageSize; index < rowCount; index++)
            {
                var currentRow = rowIndex + TemplateDataRowIndex;
                var templateRow = templateSheet.Cells[TemplateDataRowIndex, ColumnIndex, TemplateDataRowIndex, ColumnTemplateCount];
                var destinationRow = currentSheet.Cells[currentRow, ColumnIndex, currentRow, ColumnTemplateCount];
                templateRow.Copy(destinationRow);

                var customerWinloss = dataList.ElementAt(index);
                currentSheet.SetValue(currentRow, ColumnIndex, index + 1);
                currentSheet.SetValue(currentRow, ColumnExternalId, customerWinloss.ExternalId);
                currentSheet.SetValue(currentRow, ColumnSystemId, customerWinloss.SystemId);
                currentSheet.SetValue(currentRow, ColumnTurnover, customerWinloss.Turnover);
                currentSheet.SetValue(currentRow, ColumnTotalTransaction, customerWinloss.TotalTransaction);
                currentSheet.SetValue(currentRow, ColumnCustomerWl, customerWinloss.CustomerWL);
                currentSheet.SetValue(currentRow, ColumnCustomerComm, customerWinloss.PlayerComm);
                currentSheet.SetValue(currentRow, ColumnCustomerTotal, customerWinloss.CustomerWL + customerWinloss.PlayerComm);
                currentSheet.SetValue(currentRow, ColumnBetEffective, customerWinloss.BetEffective);
                currentSheet.SetValue(currentRow, ColumnLicenseeWl, customerWinloss.LicenseeWL);
                currentSheet.SetValue(currentRow, ColumnBaCommission, customerWinloss.BAComm);
                currentSheet.SetValue(currentRow, ColumnAmtDueToBa, customerWinloss.AmtDueToBA);

                rowIndex++;
            }

            AssignSumData(currentSheet, templateSheet, data.TotalData, rowIndex);
        }

        private static void ProcessUI(ExcelWorksheet currentSheet, CustomerWinLossMetaModel metaModel)
        {
            if (!metaModel.SystemIdVisible)
            {
                currentSheet.Column(ColumnSystemId).Hidden = true;
            }

            if (!metaModel.IsAllowedCustomerComm)
            {
                currentSheet.Column(ColumnCustomerTotal).Hidden = true;
                currentSheet.Column(ColumnCustomerComm).Hidden = true;

                var customerWLCell = currentSheet.Cells[StartTemplateHeaderRowIndex, ColumnCustomerWl, StartTemplateHeaderRowIndex, ColumnCustomerComm];
                customerWLCell.Merge = false;
                currentSheet.Row(EndTemplateHeaderRowIndex).Hidden = true;
            }

            if (!metaModel.IsAllowBA)
            {
                currentSheet.Column(ColumnBaCommission).Hidden = true;
                currentSheet.Column(ColumnAmtDueToBa).Hidden = true;
            }
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

        private static void AssignSumData(ExcelWorksheet currentSheet, ExcelWorksheet templateSheet, CustomerWinLoss totalData, int numberOfDataRow)
        {
            var rowTotalNumber = checked(numberOfDataRow + TemplateDataRowIndex);

            var templateRow = templateSheet.Cells[TemplateTotalRowIndex, ColumnIndex, TemplateTotalRowIndex, ColumnTemplateCount];
            var destinationRow = currentSheet.Cells[rowTotalNumber, ColumnIndex, rowTotalNumber, ColumnTemplateCount];
            templateRow.Copy(destinationRow);

            currentSheet.SetValue(rowTotalNumber, ColumnTurnover, totalData.Turnover);
            currentSheet.SetValue(rowTotalNumber, ColumnTotalTransaction, totalData.TotalTransaction);
            currentSheet.SetValue(rowTotalNumber, ColumnCustomerWl, totalData.CustomerWL);
            currentSheet.SetValue(rowTotalNumber, ColumnCustomerComm, totalData.PlayerComm);
            currentSheet.SetValue(rowTotalNumber, ColumnCustomerTotal, totalData.CustomerWL + totalData.PlayerComm);
            currentSheet.SetValue(rowTotalNumber, ColumnBetEffective, totalData.BetEffective);
            currentSheet.SetValue(rowTotalNumber, ColumnLicenseeWl, totalData.LicenseeWL);
            currentSheet.SetValue(rowTotalNumber, ColumnBaCommission, totalData.BAComm);
            currentSheet.SetValue(rowTotalNumber, ColumnAmtDueToBa, totalData.AmtDueToBA);

            var mergedCellRange = currentSheet.Cells[rowTotalNumber, ColumnIndex, rowTotalNumber, ColumnSystemId];
            mergedCellRange.Value = ViewResources.Total;
            mergedCellRange.Merge = true;
        }
    }
}