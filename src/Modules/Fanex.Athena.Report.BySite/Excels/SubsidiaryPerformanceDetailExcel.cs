using System;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class SubsidiaryPerformanceDetailExcel : BaseExcel, ISubsidiaryPerformanceDetailExcel
    {
        private const int FirstColumn = 1;

        protected const string SheetName = "SubsidiaryPerformance";

        protected const int TemplateSheetIndex = 0;

        protected const int TemplateTitleRowIndex = 1;

        protected const int TemplateHeaderRowIndex = 2;

        protected const int TemplateDataRowIndex = 3;

        protected const int ColumnIndex = 1;

        protected const int ColumnExternalId = 2;

        protected const int ColumnSystemId = 3;

        protected const int ColumnCurrency = 4;

        protected const int ColumnBetCount = 5;

        protected const int ColumnTurnover = 6;

        protected const int ColumnCustomerWinLoss = 7;

        protected const int ColumnSubsidiaryWinLoss = 8;

        protected string[] Headers =
        {
            "#",
            ViewResources.ExternalId,
            ViewResources.SystemId,
            ViewResources.Currency,
            ViewResources.BetCount,
            ViewResources.Turnover,
            ViewResources.CustomerW_L,
            ViewResources.SubsidiaryWL
        };

        protected SubsidiaryPerformanceMetaModel MetaModel;

        public SubsidiaryPerformanceDetailExcel(IFileService fileService, IOptions<ExcelOptions> excelOptionsAccessor,
                                                IWebHostEnvironment environment) : base(environment, fileService, excelOptionsAccessor) { }

        protected ReportResultModel<SubsidiaryPerformanceDetail> Data { get; set; }

        public string Export(string template, ReportResultModel<SubsidiaryPerformanceDetail> data, SubsidiaryPerformanceMetaModel metaModel)
        {
            MetaModel = metaModel;
            Data = data;

            return CreateFileStream(template);
        }

        private string CreateFileStream(string template)
        {
            var fileInfo = new FileInfo(template);
            using var package = new ExcelPackage(fileInfo, false);

            var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
            var currentSheet = package.Workbook.Worksheets.Add(SheetName, templateSheet);

            AssignTitle(currentSheet);
            AssignHeader(currentSheet);
            AssignSheetData(currentSheet);
            AssignTotalRow(currentSheet);

            package.Workbook.Worksheets.Delete(TemplateSheetIndex);
            var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");

            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            package.SaveAs(stream);

            return fileName;
        }

        private void AssignTitle(ExcelWorksheet sheet)
        {
            var title = $"{ViewResources.SubsidiaryPerformance}, {MetaModel.FromDate:MM/dd/yyyy} - {MetaModel.ToDate:MM/dd/yyyy}, {ViewResources.BaseCurrency}: {MetaModel.BaseCurrency}";

            sheet.Cells[TemplateTitleRowIndex, FirstColumn, TemplateTitleRowIndex, Headers.Length].Merge = true;
            sheet.SetValue(TemplateTitleRowIndex, FirstColumn, title);
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
            if (Data.DataList.Count() > 1)
            {
                sheet.InsertRow(TemplateDataRowIndex + 1, Data.DataList.Count() - 1, TemplateDataRowIndex);
            }

            var rowIndex = 0;

            foreach (var rowData in Data.DataList)
            {
                var realIndex = rowIndex + TemplateDataRowIndex;
                sheet.SetValue(realIndex, ColumnIndex, rowIndex + 1);
                sheet.SetValue(realIndex, ColumnExternalId, rowData.ExternalId);
                sheet.SetValue(realIndex, ColumnSystemId, rowData.SystemId);
                sheet.SetValue(realIndex, ColumnCurrency, rowData.CurrencyName);

                AssignCommonData(sheet, rowData, realIndex);

                rowIndex++;
            }
        }

        private void AssignTotalRow(ExcelWorksheet sheet)
        {
            var totalRowIndex = Data.DataList.Count() + TemplateDataRowIndex;
            if (Data.SummaryTotal.Count > 1)
            {
                sheet.InsertRow(totalRowIndex + 1, Data.SummaryTotal.Count - 1, totalRowIndex);
            }

            foreach (var totalData in Data.SummaryTotal)
            {
                sheet.Cells[totalRowIndex, ColumnIndex, totalRowIndex, ColumnSystemId].Merge = true;
                sheet.Cells[totalRowIndex, ColumnIndex].Value = string.Format(ViewResources.Totalin, MetaModel.BaseCurrency);

                sheet.SetValue(totalRowIndex, ColumnCurrency, totalData.Value.CurrencyName);

                AssignCommonData(sheet, totalData.Value, totalRowIndex);

                totalRowIndex++;
            }

            sheet.Cells[totalRowIndex, ColumnIndex, totalRowIndex, ColumnCurrency].Merge = true;
            sheet.Cells[totalRowIndex, ColumnIndex].Value = string.Format(ViewResources.Totalin, MetaModel.BaseCurrency);

            AssignCommonData(sheet, Data.TotalData, totalRowIndex);
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
