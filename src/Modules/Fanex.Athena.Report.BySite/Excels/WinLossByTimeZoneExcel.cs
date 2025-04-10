namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Common.Excel;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;
    using OfficeOpenXml;
    using Resources;

    public class WinLossByTimeZoneExcel : BaseExcel, IWinLossByTimeZoneExcel
    {
        private const string SheetName = "WinLossByTimeZone";

        private const int ReportBySummary = 1;

        private const int ReportDaily = 2;

        private const int CurrencyGroup = 2;

        private const int Percentage = 100;

        private const int TemplateSheetIndex = 0;

        private const int TemplateTitleRowIndex = 1;

        private const int TemplateHeaderRowIndex = 2;

        private const int TemplateDataRowIndex = 3;

        private const int ColumnIndex = 1;

        public const int ColumnReportBy = 2;

        public const int ColumnGroupBy = 3;

        private const int ColumnBetCount = 4;

        private const int ColumnTurnOver = 5;

        private const int ColumnCustomerComm = 6;

        private const int ColumnLicenseeWl = 7;

        private const int ColumnLicenseeComm = 8;

        private const int ColumnLicenseeNetWl = 9;

        private const int ColumnMargin = 10;

        public const int HeaderReportBy = 1;

        public const int HeaderGroupBy = 2;

        private readonly string[] _headerTextList =
        {
            "#",
            "",
            "",
            ViewResources.BetCount,
            ViewResources.Turnover,
            ViewResources.CustomerComm,
            ViewResources.LicenseeWL,
            ViewResources.LicenseeComm,
            ViewResources.LicenseeNetWL,
            ViewResources.Margin
        };

        private IEnumerable<WinLossByTimeZone> _data;

        private WinLossByTimeZoneMetaModel _metaModel;

        private WinLossByTimeZone _totalData;

        public WinLossByTimeZoneExcel(
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IWebHostEnvironment environment) : base(environment, fileService,
            excelOptionsAccessor)
        {
        }

        public string Export(
            string template,
            ReportResultModel<WinLossByTimeZone> data,
            WinLossByTimeZoneMetaModel metaModel)
        {
            _metaModel = metaModel;
            _data = data.DataList;
            _totalData = data.TotalData;

            return CreateFileStream(template);
        }

        public string CreateFileStream(string template)
        {
            var fileInfo = new FileInfo(template);
            using var package = new ExcelPackage(fileInfo, false);

            var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
            var currentSheet = package.Workbook.Worksheets.Add(SheetName, templateSheet);

            AssignTitle(currentSheet);
            AssignHeader(currentSheet);
            AssignSheetData(currentSheet);
            ProcessUi(currentSheet);

            package.Workbook.Worksheets.Delete(TemplateSheetIndex);
            var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");

            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            package.SaveAs(stream);

            return fileName;
        }

        protected void AssignTitle(ExcelWorksheet sheet)
        {
            var destinationCell = sheet.Cells[TemplateTitleRowIndex, ColumnIndex, TemplateTitleRowIndex, _headerTextList.Length];
            destinationCell.Merge = true;

            sheet.SetValue(TemplateTitleRowIndex, ColumnIndex, BuildExcelTitle());
        }

        protected void AssignHeader(ExcelWorksheet sheet)
        {
            // Config dynamic headers, column start from 1 => 
            _headerTextList[HeaderReportBy] =
                _metaModel.ReportBy == ReportDaily ? ViewResources.Date : ViewResources.Month;
            _headerTextList[HeaderGroupBy] =
                _metaModel.GroupBy == CurrencyGroup ? ViewResources.Currency : ViewResources.Product;

            for (var i = 1; i <= _headerTextList.Length; i++)
            {
                sheet.SetValue(TemplateHeaderRowIndex, i, _headerTextList[i - 1]);
            }

            sheet.View.FreezePanes(TemplateDataRowIndex, ColumnIndex);
        }

        protected void AssignSheetData(ExcelWorksheet sheet)
        {
            var rowIndex = 0;

            if (_data.Count() > 1)
            {
                sheet.InsertRow(TemplateDataRowIndex + 1, _data.Count() - 1, TemplateDataRowIndex);
            }

            foreach (var rowData in _data)
            {
                var realIndex = rowIndex + TemplateDataRowIndex;
                sheet.SetValue(realIndex, ColumnIndex, rowIndex + 1);

                AssignCommonData(sheet, rowData, realIndex);

                rowIndex++;
            }

            AssignTotalRow(sheet, rowIndex + TemplateDataRowIndex);
        }

        protected virtual void AssignTotalRow(ExcelWorksheet sheet, int totalRow)
        {
            AssignCommonData(sheet, _totalData, totalRow);

            sheet.Cells[totalRow, ColumnIndex, totalRow, ColumnGroupBy].Merge = true;
            sheet.Cells[totalRow, ColumnIndex].Value = string.Format(ViewResources.Totalin, _metaModel.BaseCurrency);
        }

        protected void ProcessUi(ExcelWorksheet sheet)
        {
            if (_metaModel.ReportBy == ReportBySummary)
            {
                sheet.Column(ColumnReportBy).Hidden = true;
            }
        }

        private string BuildExcelTitle()
        {
            var reportTitle = string.Format(
                "{0}, {1}, {2:MM/dd/yyyy} → {3:MM/dd/yyyy}, {4}: {5}",
                ViewResources.WinLossByTimeZone,
                _metaModel.TimeZone > 0 ? $"GMT+{_metaModel.TimeZone}" : $"GMT{_metaModel.TimeZone}",
                _metaModel.FromDate,
                _metaModel.ToDate,
                ViewResources.Currency,
                _metaModel.CurrencyName);

            if (_metaModel.IsConvertAll)
            {
                reportTitle = $"{reportTitle}, {ViewResources.BaseCurrency}: {_metaModel.BaseCurrency}";
            }

            return reportTitle;
        }

        private void AssignCommonData(ExcelWorksheet sheet, WinLossByTimeZone rowData, int rowIndex)
        {
            sheet.SetValue(rowIndex, ColumnReportBy,
                rowData.ReportBy.ToString(_metaModel.ReportBy == ReportDaily ? "MM/dd/yyyy" : "MM/yyyy"));
            sheet.SetValue(rowIndex, ColumnGroupBy, rowData.GroupBy);
            sheet.SetValue(rowIndex, ColumnBetCount, rowData.BetCount);
            sheet.SetValue(rowIndex, ColumnTurnOver, rowData.Turnover);
            sheet.SetValue(rowIndex, ColumnCustomerComm, rowData.CustomerComm);
            sheet.SetValue(rowIndex, ColumnLicenseeWl, rowData.LicenseeWinLoss);
            sheet.SetValue(rowIndex, ColumnLicenseeComm, rowData.LicenseeComm);
            sheet.SetValue(rowIndex, ColumnLicenseeNetWl, rowData.LicenseeNetWinLoss);
            sheet.SetValue(rowIndex, ColumnMargin, rowData.Margin / Percentage);

            if (rowData.LicenseeWinLoss < 0)
            {
                sheet.Cells[rowIndex, ColumnLicenseeWl].Style.Font.Color.SetColor(Color.Red);
            }

            if (rowData.LicenseeNetWinLoss < 0)
            {
                sheet.Cells[rowIndex, ColumnLicenseeNetWl].Style.Font.Color.SetColor(Color.Red);
            }
        }
    }
}