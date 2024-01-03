namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;
    using NPOI.SS.UserModel;

    public class DailyBetsExcel : BaseExcel, IDailyBetsExcel
    {
        private readonly IDailyBetsRepository _repository;
        private readonly IDailyBetsService _service;
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";
        private string _tempFolderPath;
        private int _totalRows;
        private AuditTicket _totalData;

        private const int ColumnIndex = 0;
        private const int ColumnPlayerName = 1;
        private const int ColumnCurrency = 2;
        private const int ColumnRefNo = 3;
        private const int ColumnTransTime = 4;
        private const int ColumnProduct = 5;
        private const int ColumnSport = 6;
        private const int ColumnBetType = 7;
        private const int ColumnTurnover = 8;
        private const int ColumnPayout = 9;
        private const int ColumnWL = 10;
        private const int ColumnTurnoverUSD = 11;
        private const int ColumnPayoutUSD = 12;
        private const int ColumnWLUSD = 13;
        private const int ColumnStatus = 14;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 15;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalCurrencyTemplate = 5;

        private readonly IExcelBuilder _builder;

        public DailyBetsExcel(
            IExcelBuilder builder,
            IDailyBetsRepository dailyBetsRepository,
            IDailyBetsService dailyBetsService,
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IWebHostEnvironment enviroment)
            : base(enviroment, fileService, excelOptionsAccessor)
        {
            _builder = builder;
            _repository = dailyBetsRepository;
            _service = dailyBetsService;
        }

        public async Task<string> Export(string templatePath, DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList)
        {
            var files = new List<ZipEntry>();

            metaModel.PageIndex = 1;
            metaModel.PageSize = ExcelOptions.MaximumRowPerFile;

            var (totalRows, totalData) = await _repository.GetTotalDataAsync(metaModel).ConfigureAwait(false);
            if (totalRows == 0)
            {
                return null;
            }

            _totalRows = totalRows;
            _totalData = totalData;
            _tempFolderPath = EnsureFolderPath();

            var totalPage = (totalRows - 1) / ExcelOptions.MaximumRowPerFile + 1;
            var nextPage = metaModel.PageIndex;
            var exportTasks = new List<Task<ZipEntry>>();

            while (nextPage <= totalPage)
            {
                var pageMetaModel = metaModel.ShallowCopy();
                pageMetaModel.PageIndex = nextPage;
                exportTasks.Add(ExportOnePageAsync(pageMetaModel, auditCurrencyList, templatePath));
                ++nextPage;
            }

            var fileNames = await Task.WhenAll(exportTasks).ConfigureAwait(false);
            files.AddRange(fileNames);

            return ZipExcelFile(files, _tempFolderPath);
        }

        private async Task<ZipEntry> ExportOnePageAsync(DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList, string templatePath)
        {
            var data = await _service.GetDataAsync(metaModel, auditCurrencyList);
            var workBook = GetWorkbook(templatePath, data.DataList, metaModel);

            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return WriteExcelFile(stream.ToArray(), $"DailyBets_{metaModel.PageIndex}");
        }

        private ZipEntry WriteExcelFile(byte[] excelFile, string fileName)
        {
            var excelFilePath = Path.Combine(_tempFolderPath, $"{Guid.NewGuid()}.dat");
            FileService.WriteAllBytes(excelFilePath, excelFile);

            return new ZipEntry
            {
                Path = excelFilePath,
                FileName = $"{fileName}.xls"
            };
        }

        public IWorkbook GetWorkbook(string template, IEnumerable<AuditTicket> dataList, DailyBetsMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet, metaModel);

            if (dataList.Any())
            {
                AssignSheetData(sheet, templateSheet, dataList);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            HideColumnsBySite(sheet, metaModel);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private void HideColumnsBySite(ISheet sheet, DailyBetsMetaModel metaModel)
        {
            if (!string.Equals(metaModel.SiteName, "12B", StringComparison.InvariantCultureIgnoreCase))
            {
                _builder.HideColumn(sheet, ColumnPayout);
                _builder.HideColumn(sheet, ColumnPayoutUSD);
            }
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);

            var headerTextList = new[]
            {
                "#",
                ViewResources.PlayerName,
                ViewResources.Currency,
                ViewResources.RefNo,
                ViewResources.TransTime,
                ViewResources.Product,
                ViewResources.Sport,
                ViewResources.BetType,
                ViewResources.Turnover,
                ViewResources.Payout,
                ViewResources.WinLoss,
                $"{ViewResources.Turnover} (USD)",
                $"{ViewResources.Payout} (USD)",
                $"{ViewResources.WinLoss} (USD)",
                ViewResources.Status
            };

            SetTextRowHeader(row, headerTextList);
        }

        private static void SetTextRowHeader(IRow row, IEnumerable<string> headerTextList)
        {
            for (int i = 0; i < headerTextList.Count(); i++)
            {
                row.Cells[i].SetCellValue(headerTextList.ElementAt(i));
            }
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, IEnumerable<AuditTicket> dataList)
        {
            var index = 0;

            foreach (var item in dataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                ++index;
                AssignCellData(row, item);
            }

            AssignTotalCurrency(sheet, templateSheet, index + RowContentTemplate);
        }

        private void AssignTotalCurrency(ISheet sheet, ISheet templateSheet, int index)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowTotalCurrencyTemplate, index);
            row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, "USD"));
            row.Cells[ColumnTurnoverUSD].SetCellValue((double)_totalData.TurnoverBased);
            row.Cells[ColumnPayoutUSD].SetCellValue((double)_totalData.PayoutBased);
            row.Cells[ColumnWLUSD].SetCellValue((double)_totalData.WinLossBased);

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnWL);
        }

        private static void AssignCellData(IRow row, AuditTicket item)
        {
            row.Cells[ColumnIndex].SetCellValue(item.Index);
            row.Cells[ColumnPlayerName].SetCellValue(item.PlayerName);
            row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
            row.Cells[ColumnRefNo].SetCellValue(item.RefNo);
            row.Cells[ColumnTransTime].SetCellValue(item.TransTimeString);
            row.Cells[ColumnProduct].SetCellValue(item.ProductName);
            row.Cells[ColumnSport].SetCellValue(item.SportName);
            row.Cells[ColumnBetType].SetCellValue(item.BetType);
            row.Cells[ColumnTurnover].SetCellValue((double)item.Turnover);
            row.Cells[ColumnPayout].SetCellValue((double)item.Payout);
            row.Cells[ColumnWL].SetCellValue((double)item.WinLoss);
            row.Cells[ColumnTurnoverUSD].SetCellValue((double)item.TurnoverBased);
            row.Cells[ColumnPayoutUSD].SetCellValue((double)item.PayoutBased);
            row.Cells[ColumnWLUSD].SetCellValue((double)item.WinLossBased);
            row.Cells[ColumnStatus].SetCellValue(item.Status);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, DailyBetsMetaModel metaModel)
        {
            var currencyIdSelectedList = metaModel.CurrencyIdList.Split(',').Select(int.Parse);
            var displayedCurrency = metaModel.CurrencyList.Count == currencyIdSelectedList.Count()
                ? ViewResources.AllCurrencies
                : string.Join(",", metaModel.CurrencyList.Where(_ => currencyIdSelectedList.Contains(_.Key)).Select(_ => _.Value));
            var totalTicketText = $"{ViewResources.TotalTickets}: {_totalRows}";

            var reportTitle = $"{ViewResources.DailyBets}, GMT+8, {metaModel.Date:dd/MM/yyyy} - {displayedCurrency} - {totalTicketText}";

            _builder.SetRowValue(
                sheet,
                templateSheet,
                sourceRowIndex: RowTitleTemplate,
                destRowIndex: RowTitleTemplate,
                columnCount: ColumnTemplateCount,
                value: reportTitle);

            sheet.CreateFreezePane(0, RowContentTemplate);

            _builder.ClearTemplate(sheet, RowContentTemplate);
        }
    }
}