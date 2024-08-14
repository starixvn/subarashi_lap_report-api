namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Excel;
    using Common.Excel.Interfaces;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Options;
    using Resources;

    public class AgCasinoBalanceExcel : IAgCasinoBalanceExcel
    {
        public const int MaxRowCount = 30000;

        private const string SheetName = "Data";

        private const string TemplateSheetName = "Template";

        private const int ColumnTemplateCount = 12;

        private const int ColumnIndex = 0;

        private const int ColumnDate = 1;

        private const int ColumnExternalId = 2;

        private const int ColumnSystemId = 3;

        private const int ColumnCurrency = 4;

        private const int ColumnOpeningBalance = 5;

        private const int ColumnTransferIn = 6;

        private const int ColumnTransferOut = 7;

        private const int ColumnTurnover = 8;

        private const int ColumnTotalTransaction = 9;

        private const int ColumnCustomerWl = 10;

        private const int ColumnClosingBalance = 11;

        public const int RowTitleTemplate = 0;

        public const int RowHeaderTemplate = 1;

        private const int RowContentTemplate = 2;

        private const int RowTotalNoSolidBorder = 4;

        private const int RowTotalSolidBorder = 3;

        private const int RowGrantTotal = 5;

        private const int RowNoInfoTemplate = 6;

        private readonly IExcelBuilder _builder;

        private SortableList<AgCasinoBalance> _data;

        private ThirdPartyWinLossMetaModel _metaModel;

        private ThirdPartyWinLossExcelOption _option;

        private int _pageSize;

        private IDictionary<string, AgCasinoBalance> _summaryTotal;

        public AgCasinoBalanceExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        protected internal int PageSize
        {
            get => _pageSize;
            set => _pageSize = value is 0 or > MaxRowCount ? MaxRowCount : value;
        }

        public byte[] Export(
            string template,
            SortableReportResultModel<AgCasinoBalance> data,
            ThirdPartyWinLossMetaModel metaModel,
            ThirdPartyWinLossExcelOption option)
        {
            _data = data.DataList;
            _summaryTotal = data.SummaryTotal;
            _metaModel = metaModel;
            _option = option;

            PageSize = _metaModel.PageSize;
            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        protected internal virtual int AssignTotalData(ISheet sheet, ISheet templateSheet, int rowIndex)
        {
            var beginTotalRow = rowIndex;
            var totalCurrencyList = GetTotalCurrency(_data);

            foreach (var totalCurrency in totalCurrencyList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowTotalNoSolidBorder, rowIndex);

                if (rowIndex == beginTotalRow)
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalSolidBorder, rowIndex);
                    row.Cells[0].SetCellValue(ViewResources.Total);
                }

                row.Cells[ColumnCurrency].SetCellValue(totalCurrency.CurrencyName);

                AssignCommonData(row, totalCurrency, true);

                rowIndex++;
            }

            // Merge first two column
            const int NumberOfTotalMergedCol = 3;
            _builder.MergeCells(sheet, rowIndex - totalCurrencyList.Count(), rowIndex - 1, 0, NumberOfTotalMergedCol);

            return rowIndex;
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);
            row.Cells[ColumnIndex].SetCellValue("#");
            row.Cells[ColumnDate].SetCellValue(ViewResources.Date);
            row.Cells[ColumnExternalId].SetCellValue(ViewResources.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(ViewResources.SystemId);
            row.Cells[ColumnCurrency].SetCellValue(ViewResources.Currency);
            row.Cells[ColumnOpeningBalance].SetCellValue(ViewResources.OpeningBalance);
            row.Cells[ColumnTransferIn].SetCellValue(ViewResources.TransferIn);
            row.Cells[ColumnTransferOut].SetCellValue(ViewResources.TransferOut);
            row.Cells[ColumnTurnover].SetCellValue(ViewResources.Turnover);
            row.Cells[ColumnTotalTransaction].SetCellValue(ViewResources.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.PlayerWL);
            row.Cells[ColumnClosingBalance].SetCellValue(ViewResources.ClosingBalance);
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet tempateSheet)
        {
            _builder.SetRowValue(sheet, tempateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount,
                ViewResources.NoInfo);
        }

        protected void AssignSheetData(ISheet sheet, ISheet templateSheet, int pageIndex)
        {
            var rowCount = Math.Min(_data.Count, checked((pageIndex + 1) * PageSize));
            var rowIndex = RowContentTemplate;
            int index;

            for (index = pageIndex * PageSize; index < rowCount; index++)
            {
                var casinoBalance = _data[index];
                var isDrillDownLevel = casinoBalance.RowSpan == 0;

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, rowIndex);

                var reportDateString = casinoBalance.Date.Value.ToString("MM/dd/yyyy");

                row.Cells[ColumnDate].SetCellValue(reportDateString);
                row.Cells[ColumnExternalId].SetCellValue(casinoBalance.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(casinoBalance.SystemId);
                row.Cells[ColumnCurrency].SetCellValue(casinoBalance.CurrencyName);
                AssignCommonData(row, casinoBalance);
                rowIndex++;

                row.Cells[ColumnIndex].SetCellValue(isDrillDownLevel ? index + 1 : casinoBalance.Index);

                if (casinoBalance.RowSpan > 1)
                {
                    var currentRow = index + RowContentTemplate;
                    _builder.MergeRows(sheet, currentRow, currentRow + casinoBalance.RowSpan - 1, ColumnIndex);
                    _builder.MergeRows(sheet, currentRow, currentRow + casinoBalance.RowSpan - 1, ColumnDate);
                }
            }

            rowIndex = AssignTotalData(sheet, templateSheet, rowIndex);
            AssignGrandTotalData(sheet, templateSheet, rowIndex);
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var currencyTitle = ExcelHelper.GetTitleCurrencyName(_option);
            var reportTitle =
                $"{ViewResources.AGCasinoBalance}, {_metaModel.FromDate:MM/dd/yyyy} --> {_metaModel.ToDate:MM/dd/yyyy}, {currencyTitle}";
            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount,
                reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        protected void ProcessUi(ISheet sheet)
        {
            switch (_metaModel.DrillDownLevel)
            {
                case 0:
                    _builder.HideColumn(sheet, ColumnExternalId, ColumnSystemId);
                    break;

                case 1:
                    _builder.HideColumn(sheet, ColumnExternalId, ColumnDate);
                    break;

                default:
                    _builder.HideColumn(sheet, ColumnDate);
                    break;
            }
        }

        protected int AssignGrandTotalData(ISheet sheet, ISheet templateSheet, int rowIndex)
        {
            var currencyList = _summaryTotal.Keys.ToList();
            currencyList.Reverse();

            foreach (var currency in currencyList)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotal, rowIndex);
                AssignCommonData(row, _summaryTotal[currency], true);
                row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, currency.ToUpperInvariant()));

                // Merge first two column
                _builder.MergeCols(sheet, ColumnIndex, ColumnCurrency, rowIndex);
                rowIndex++;
            }

            return rowIndex;
        }

        protected static int GetNumberOfSheets(int numberOfItems, int pageSize) =>
            numberOfItems % pageSize == 0 ? numberOfItems / pageSize : numberOfItems / pageSize + 1;

        private IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            if (_data.Any())
            {
                var numberOfSheets = GetNumberOfSheets(_data.Count, PageSize);

                for (var i = 0; i < numberOfSheets; i++)
                {
                    var sheet = _builder.CloneSheet(workBook, TemplateSheetName, "Data" + (i + 1));

                    AssignTitle(sheet, templateSheet);
                    AssignHeader(sheet, templateSheet);
                    AssignSheetData(sheet, templateSheet, i);
                    ProcessUi(sheet);
                }
            }
            else
            {
                var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);

                AssignTitle(sheet, templateSheet);
                AssignHeader(sheet, templateSheet);
                AssignNoInfoRow(sheet, templateSheet);
                ProcessUi(sheet);
            }

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private static void AssignCommonData(IRow row, AgCasinoBalance data, bool isTotalData = false)
        {
            row.Cells[ColumnTransferIn].SetCellValue((double)data.TransferIn);
            row.Cells[ColumnTransferOut].SetCellValue((double)data.TransferOut);
            row.Cells[ColumnTurnover].SetCellValue((double)data.TurnOver);
            row.Cells[ColumnTotalTransaction].SetCellValue((double)data.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue((double)data.CustomerWinLoss);

            if (isTotalData)
            {
                row.Cells[ColumnOpeningBalance].SetCellValue("-");
                row.Cells[ColumnClosingBalance].SetCellValue("-");
            }
            else
            {
                row.Cells[ColumnOpeningBalance].SetCellValue((double)data.OpeningBalance);
                row.Cells[ColumnClosingBalance].SetCellValue((double)data.ClosingBalance);
            }
        }

        private static IEnumerable<AgCasinoBalance> GetTotalCurrency(IEnumerable<AgCasinoBalance> dataList)
        {
            var listTotalCurrency = dataList.GroupBy(x => new { x.CurrencyName })
                .Select(group => new AgCasinoBalance
                {
                    CurrencyName = group.Key.CurrencyName,
                    OpeningBalance = group.Sum(a => a.OpeningBalance),
                    TransferIn = group.Sum(a => a.TransferIn),
                    TransferOut = group.Sum(a => a.TransferOut),
                    TurnOver = group.Sum(a => a.TurnOver),
                    TotalTransaction = group.Sum(a => a.TotalTransaction),
                    CustomerWinLoss = group.Sum(a => a.CustomerWinLoss),
                    LicenseeWL = group.Sum(a => a.LicenseeWL),
                    ClosingBalance = group.Sum(a => a.ClosingBalance)
                }).ToList();

            return listTotalCurrency;
        }
    }
}