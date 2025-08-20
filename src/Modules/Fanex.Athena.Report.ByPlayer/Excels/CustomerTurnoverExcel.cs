namespace Fanex.Athena.Report.ByPlayer.Excels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Excel;
    using Common.Excel.Interfaces;
    using Common.Model.MetaModels;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using DTO;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Options;
    using Resources;

    public class CustomerTurnoverExcel : ICustomerTurnoverExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        private const string FileName = "CustomerTurnover";

        protected const int ColumnIndex = 0;

        private const int ColumnDate = 1;

        private const int ColumnExternalId = 2;

        private const int ColumnSystemId = 3;

        private const int ColumnCurrency = 4;

        private const int ColumnSport = 5;

        private const int ColumnBetType = 6;

        private const int ColumnEffectiveBettingValue = 7;

        private const int ColumnTotalTransaction = 8;

        private const int ColumnTurnover = 9;

        private const int ColumnTotalTurnover = 10;

        private const int ColumnCustomerWl = 11;

        private const int ColumnCustomerComm = 12;

        private const int ColumnCustomerTotal = 13;

        private const int ColumnTemplateCount = 14;

        public const int RowTitleTemplate = 0;

        public const int RowHeaderTemplate = 1;

        private const int RowContentTemplate = 3;

        private const int RowNoInfoTemplate = 5;

        private const int RowTableHeaderTemplate = 1;

        private const int RowTotalTemplate = 4;

        private readonly IExcelBuilder _builder;

        private readonly ICompressService _compressService;

        private readonly IFileService _fileService;

        private CustomerTurnoverExcelOptions _options;

        private CustomerTurnoverMetaModel _metaModel;

        public CustomerTurnoverExcel(IExcelBuilder builder, ICompressService compressService, IFileService fileService)
        {
            _builder = builder;
            _compressService = compressService;
            _fileService = fileService;
        }

        public List<ZipEntry> ExportZip(string template, IEnumerable<ReportResultModel<CustTurnover>> dataList, CustomerTurnoverMetaModel metaModel, CustomerTurnoverExcelOptions excelOption)
        {
            var listExcel = new List<ZipEntry>();
            var pageIndex = 0;
            foreach (var data in dataList)
            {
                var excel = ExportFile(template, data, metaModel, excelOption);
                var entry = WriteExcelFile(excel, $"{FileName}_{pageIndex}");
                listExcel.Add(entry);
                pageIndex++;
            }

            return listExcel;
        }

        public byte[] ExportFile(string template, ReportResultModel<CustTurnover> data, CustomerTurnoverMetaModel metaModel, CustomerTurnoverExcelOptions excelOption)
        {
            _metaModel = metaModel;
            _options = excelOption;

            var workBook = GetWorkbook(template, data);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        private IWorkbook GetWorkbook(string template, ReportResultModel<CustTurnover> data)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignTitle(sheet, templateSheet);
            AssignHeader(sheet, templateSheet);

            if (data.DataList.Any())
            {
                AssignSheetData(sheet, templateSheet, data.DataList, data.TotalData);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            ProcessUi(sheet);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);
            var headerTextList = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Currency,
                ViewResources.Sport,
                ViewResources.BetType,
                ViewResources.EffectiveBettingValue,
                ViewResources.TotalTransaction,
                ViewResources.Turnover,
                ViewResources.TotalTurnover,
                ViewResources.CustomerW_L,
            };

            ExcelHelper.SetTextRowHeader(row, headerTextList);
            row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate + 1, RowHeaderTemplate + 1);

            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.WinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        protected void AssignSheetData(ISheet sheet, ISheet templateSheet, IEnumerable<CustTurnover> data, CustTurnover totalData)
        {
            var rowIndex = 0;
            foreach (var customerTurnover in data)
            {
                var realIndex = rowIndex + RowContentTemplate;
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, realIndex);
                if (customerTurnover.RowSpan > -1)
                {
                    row.Cells[ColumnIndex].SetCellValue(customerTurnover.Index);
                    row.Cells[ColumnDate].SetCellValue(customerTurnover.ReportBy.ToString(_options.FormatDateTime));
                    row.Cells[ColumnExternalId].SetCellValue(customerTurnover.ExternalId);
                    row.Cells[ColumnSystemId].SetCellValue(customerTurnover.SystemId);
                    row.Cells[ColumnCurrency].SetCellValue(customerTurnover.Currency);
                    row.Cells[ColumnTotalTurnover].SetCellValue((double)customerTurnover.TotalTurnover);
                }

                if (customerTurnover.RowSpan2 > -1)
                {
                    row.Cells[ColumnSport].SetCellValue(customerTurnover.SportType);
                }

                if (customerTurnover.RowSpan > 0)
                {
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan - 1) + realIndex, ColumnIndex);
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan - 1) + realIndex, ColumnDate);
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan - 1) + realIndex, ColumnExternalId);
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan - 1) + realIndex, ColumnSystemId);
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan - 1) + realIndex, ColumnCurrency);
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan - 1) + realIndex, ColumnTotalTurnover);
                }

                if (customerTurnover.RowSpan2 > 0)
                {
                    _builder.MergeRows(sheet, realIndex, (customerTurnover.RowSpan2 - 1) + realIndex, ColumnSport);
                }

                row.Cells[ColumnBetType].SetCellValue(customerTurnover.Bettype);
                row.Cells[ColumnEffectiveBettingValue].SetCellValue((double)customerTurnover.EffectiveBettingValue);
                row.Cells[ColumnTotalTransaction].SetCellValue(customerTurnover.TotalTransactions);
                row.Cells[ColumnTurnover].SetCellValue((double)customerTurnover.Turnover);
                row.Cells[ColumnCustomerWl].SetCellValue((double)customerTurnover.CustomerWinLoss);
                row.Cells[ColumnCustomerComm].SetCellValue((double)customerTurnover.CustomerComm);
                row.Cells[ColumnCustomerTotal].SetCellValue((double)(customerTurnover.CustomerWinLoss + customerTurnover.CustomerComm));
                rowIndex++;
            }

            AssignTotalRow(sheet, templateSheet, totalData, rowIndex + RowContentTemplate);
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, GenerateReportTitle(_metaModel, _options.CurrencyName));

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        /// <summary>
        /// Process UI, hide redundant row and column.
        /// </summary>
        /// <param name="sheet">Excel sheet.</param>
        protected void ProcessUi(ISheet sheet)
        {
            if (!_options.CustomerCommVisible)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);
                _builder.HideRow(sheet, RowTableHeaderTemplate + 1);
            }
        }

        /// <summary>
        /// Assign data to total row.
        /// </summary>
        /// <param name="sheet">Excel sheet.</param>
        /// <param name="totalRow">The total value.</param>
        protected internal virtual void AssignTotalRow(ISheet sheet, ISheet templateSheet, CustTurnover totalData, int totalRow)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, totalRow);
            row.Cells[ColumnCurrency].SetCellValue(string.Empty);
            row.Cells[ColumnBetType].SetCellValue(string.Empty);
            row.Cells[ColumnSport].SetCellValue(string.Empty);
            row.Cells[ColumnExternalId].SetCellValue(string.Empty);
            row.Cells[ColumnSystemId].SetCellValue(string.Empty);
            row.Cells[ColumnEffectiveBettingValue].SetCellValue((double)totalData.EffectiveBettingValue);
            row.Cells[ColumnTotalTransaction].SetCellValue(totalData.TotalTransactions);
            row.Cells[ColumnTurnover].SetCellValue((double)totalData.Turnover);
            row.Cells[ColumnTotalTurnover].SetCellValue((double)totalData.Turnover);
            row.Cells[ColumnCustomerWl].SetCellValue((double)totalData.CustomerWinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue((double)totalData.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)(totalData.CustomerWinLoss + totalData.CustomerComm));

            _builder.MergeCols(sheet, ColumnIndex, ColumnBetType, totalRow);
        }

        private static string GenerateReportTitle(CustomerTurnoverMetaModel metaModel, string currencyName)
        {
            var usernameExcelTitle = string.IsNullOrEmpty(metaModel.Username) ? string.Empty : "- " + ViewResources.Username + ": " + metaModel.Username;
            var reportTitle = string.Format(
                "{0}, {1} --> {2} - {3}: {4} - {5}: {6} {7}",
                ViewResources.CustomerTurnover,
                metaModel.FromDate.ToString("MM/dd/yyyy"),
                metaModel.ToDate.ToString("MM/dd/yyyy"),
                ViewResources.Currency,
                currencyName,
                ViewResources.MinimumOdds,
                metaModel.Odds,
                usernameExcelTitle);
            return reportTitle;
        }


        private ZipEntry WriteExcelFile(byte[] data, string fileName)
        {
            var path = _compressService.EnsureFolderPath();
            var excelFilePath = Path.Combine(path, $"{Guid.NewGuid()}.xls");
            _fileService.WriteAllBytes(excelFilePath, data);
            return new ZipEntry
            {
                Path = excelFilePath,
                FileName = $"{fileName}.xls"
            };
        }

    }
}
