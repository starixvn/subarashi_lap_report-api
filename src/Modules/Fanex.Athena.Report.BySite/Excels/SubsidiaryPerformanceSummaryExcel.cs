using System;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class SubsidiaryPerformanceSummaryExcel : BaseExcel, ISubsidiaryPerformanceSummaryExcel
    {
        protected const string SheetName = "SubsidiaryPerformance";

        protected const int TemplateSheetIndex = 0;

        protected const int TemplateTitleRowIndex = 1;

        protected const int TemplateHeaderRowIndex = 2;

        protected const int TemplateDataRowIndex = 3;

        protected const int ColumnWhiteLabel = 1;

        protected const int ColumnSubsidiary = 2;

        protected const int ColumnCurrency = 3;

        protected const int ColumnProduct = 4;

        protected const int ColumnBetCount = 5;

        protected const int ColumnTurnover = 6;

        protected const int ColumnCustomerWinLoss = 7;

        protected const int ColumnSubsidiaryWinLoss = 8;

        protected string[] Headers =
        {
            ViewResources.WhiteLabel,
            ViewResources.Subsidiary,
            ViewResources.Currency,
            ViewResources.Product,
            ViewResources.BetCount,
            ViewResources.Turnover,
            ViewResources.CustomerW_L,
            ViewResources.SubsidiaryWL
        };

        protected SubsidiaryPerformanceResultModel Data;

        protected SubsidiaryPerformanceMetaModel MetaModel;

        protected SubsidiaryPerformanceExcelOption Option;

        public SubsidiaryPerformanceSummaryExcel(IFileService fileService, IOptions<ExcelOptions> excelOptionsAccessor,
                                                 IWebHostEnvironment environment) : base(environment, fileService,
            excelOptionsAccessor) { }

        public string Export(string template, SubsidiaryPerformanceResultModel data, SubsidiaryPerformanceMetaModel metaModel, SubsidiaryPerformanceExcelOption option)
        {
            MetaModel = metaModel;
            Data = data;
            Option = option;

            return CreateFileStream(template);
        }

        private string CreateFileStream(string template)
        {
            var fileInfo = new FileInfo(template);
            using var package = new ExcelPackage(fileInfo, false);

            var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
            var currentSheet = package.Workbook.Worksheets.Add(SheetName, templateSheet);

            AssignHeader(currentSheet);
            AssignTitle(currentSheet);
            AssignSheetData(currentSheet);
            AssignTotalRow(currentSheet);
            ProcessUi(currentSheet);

            package.Workbook.Worksheets.Delete(TemplateSheetIndex);
            var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");

            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            package.SaveAs(stream);

            return fileName;
        }

        private void ProcessUi(ExcelWorksheet sheet)
        {
            if (!Option.isMasterViewType)
            {
                sheet.Column(ColumnWhiteLabel).Hidden = true;
            }
        }

        private void AssignTitle(ExcelWorksheet sheet)
        {
            var title = $"{ViewResources.SubsidiaryPerformance}, {MetaModel.FromDate:MM/dd/yyyy} → {MetaModel.ToDate:MM/dd/yyyy}, {ViewResources.BaseCurrency}: {MetaModel.BaseCurrency}";

            sheet.SetValue(TemplateTitleRowIndex, ColumnWhiteLabel, title);
        }

        private void AssignHeader(ExcelWorksheet sheet)
        {
            for (var i = 0; i < Headers.Length; i++)
            {
                sheet.SetValue(TemplateHeaderRowIndex, i + 1, Headers[i]);
            }
        }

        private void AssignSheetData(ExcelWorksheet sheet)
        {
            if (Data.Data.Sum(x => x.TotalItem) > 1)
            {
                foreach (var group in Data.Data)
                {
                    sheet.InsertRow(TemplateDataRowIndex + 1, group.TotalItem - 1, TemplateDataRowIndex);
                }
            }

            var rowIndex = TemplateDataRowIndex;

            foreach (var bodyData in Data.Data)
            {
                var groupWhiteLabelCell = sheet.Cells[rowIndex, ColumnWhiteLabel, rowIndex + bodyData.TotalItem - 1, ColumnWhiteLabel];
                groupWhiteLabelCell.Value = Option.isMasterViewType ? bodyData.WhiteLabel : "";
                groupWhiteLabelCell.Merge = true;

                foreach (var groupData in bodyData.Details)
                {
                    var groupSubsidiaryCell = sheet.Cells[rowIndex, ColumnSubsidiary, rowIndex + groupData.TotalItem - 1, ColumnSubsidiary];
                    groupSubsidiaryCell.Value = groupData.Subsidiary;
                    groupSubsidiaryCell.Merge = true;

                    foreach (var groupRow in groupData.Details)
                    {
                        var groupCurrencyCell = sheet.Cells[rowIndex, ColumnCurrency,
                            rowIndex + groupRow.Value.Count() - 1, ColumnCurrency];
                        groupCurrencyCell.Value = groupRow.Key;
                        groupCurrencyCell.Merge = true;

                        foreach (var row in groupRow.Value)
                        {
                            sheet.SetValue(rowIndex, ColumnProduct, row.Product);
                            AssignCommonData(sheet, row, rowIndex);
                            rowIndex++;
                        }
                    }
                }
            }
        }

        protected virtual void AssignTotalRow(ExcelWorksheet sheet)
        {
            var totalRowIndex = Data.Data.Sum(x => x.TotalItem) + TemplateDataRowIndex;

            if (Data.Total.Count > 1)
            {
                sheet.InsertRow(totalRowIndex + 1, Data.Total.Count - 1, totalRowIndex);
            }

            foreach (var totalData in Data.Total)
            {
                var totalCellRange = sheet.Cells[totalRowIndex, ColumnWhiteLabel, totalRowIndex, ColumnSubsidiary];
                totalCellRange.Value = $"{ViewResources.Total} {totalData.Key}";
                totalCellRange.Merge = true;

                var baseCurrencyCellRange = sheet.Cells[totalRowIndex, ColumnCurrency, totalRowIndex, ColumnProduct];
                baseCurrencyCellRange.Value = $"{ViewResources.Based} {totalData.Value.CurrencyName}";
                baseCurrencyCellRange.Merge = true;

                AssignCommonData(sheet, totalData.Value, totalRowIndex);

                totalRowIndex++;
            }

            var grandTotalCellRange = sheet.Cells[totalRowIndex, ColumnWhiteLabel, totalRowIndex, ColumnSubsidiary];
            grandTotalCellRange.Value = ViewResources.Total;

            var grandTotalBaseCurrencyCellRange =
                sheet.Cells[totalRowIndex, ColumnCurrency, totalRowIndex, ColumnProduct];
            grandTotalBaseCurrencyCellRange.Value = $"{ViewResources.Based} {Data.GrandTotal.CurrencyName}";

            AssignCommonData(sheet, Data.GrandTotal, totalRowIndex);
        }

        private static void AssignCommonData(ExcelWorksheet sheet, SubsidiaryPerformanceBase data, int rowIndex)
        {
            sheet.SetValue(rowIndex, ColumnBetCount, data.BetCount);
            sheet.SetValue(rowIndex, ColumnTurnover, data.Turnover);
            sheet.SetValue(rowIndex, ColumnCustomerWinLoss, data.CustomerWinLoss);
            sheet.SetValue(rowIndex, ColumnSubsidiaryWinLoss, data.SubsidiaryWinLoss);
        }
    }
}
