using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Resources;
using NPOI.SS.UserModel;
using static Fanex.Athena.Report.BySite.Constants.ReportConstant;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class LiveReportExcel : ILiveReportExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        public const string FormatDate = "MM/dd/yyyy";

        private const int ColumnIndex = 0;

        private const int ColumnAmountDue = 8;

        private const int ColumnBetCount = 5;

        private const int ColumnCustomerComm = 2;

        private const int ColumnCustomerTotal = 3;

        private const int ColumnCustomerWl = 1;

        private const int ColumnLicenseeComm = 7;

        private const int ColumnLicenseeWl = 6;

        private const int ColumnProduct = 0;

        private const int ColumnTemplateCount = 9;

        private const int ColumnTurnover = 4;

        private const int RowTitleTemplate = 0;

        private const int RowByBetTypeHeaderTemplate = 22;

        private const int RowByBetTypeProductTitle = 25;

        private const int RowByCurrencyHeaderTemplate = 16;

        private const int RowBySportsTypeContentTemplate = 13;

        private const int RowBySportsTypeHeaderTemplate = 10;

        private const int RowContentDetailTemplate = 19;

        private const int RowContentTemplate = 4;

        private const int RowNoInfoTemplate = 7;

        private const int RowSummaryHeaderTemplate = 1;

        private const int RowTotalSummaryTemplate = 6;

        private const int SummarySectionRowIndex = RowSummaryHeaderTemplate;

        private const int SeparateSectionEmptyRow = 2;

        private readonly IExcelBuilder _builder;

        private IList<LiveReport> _summaryLiveReports;

        private LiveReportDetailResultModel _data;

        private LiveReportMetaModel _metaModel;

        private LiveReportExcelOption _options;

        public LiveReportExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, IList<LiveReport> summaryLiveReports, LiveReportDetailResultModel data,
                             LiveReportMetaModel metaModel, LiveReportExcelOption excelOption)
        {
            _data = data;
            _metaModel = metaModel;
            _options = excelOption;
            _summaryLiveReports = summaryLiveReports;

            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        private IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);

            if (_summaryLiveReports.Any())
            {
                AssignSheetData(sheet, templateSheet);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            ProcessUI(sheet);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            // Summary
            const int RowIndex = RowSummaryHeaderTemplate;
            AssignCommonHeader(sheet, templateSheet, ViewResources.Summary, ViewResources.Product,
                RowSummaryHeaderTemplate, RowIndex);
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount,
                ViewResources.NoInfo);
        }

        /// <summary>
        ///     Assign data to excel sheet.
        /// </summary>
        /// <param name="sheet">Excel sheet.</param>
        /// <param name="templateSheet"></param>
        protected void AssignSheetData(ISheet sheet, ISheet templateSheet)
        {
            var rowCount = _summaryLiveReports.Count;

            for (var index = 0; index < rowCount; index++)
            {
                var liveReport = _summaryLiveReports[index];
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);

                row.Cells[ColumnProduct].SetCellValue(liveReport.ProductName);
                row.Cells[ColumnCustomerWl].SetCellValue((double)liveReport.CustomerWinLoss);
                row.Cells[ColumnCustomerComm].SetCellValue((double)liveReport.CustomerComm);
                row.Cells[ColumnCustomerTotal].SetCellValue((double)liveReport.CustomerTotal);
                row.Cells[ColumnTurnover].SetCellValue((double)liveReport.TurnOver);
                row.Cells[ColumnBetCount].SetCellValue(liveReport.BetCount);
                row.Cells[ColumnLicenseeComm].SetCellValue((double)liveReport.LicenseeComm);
                row.Cells[ColumnLicenseeWl].SetCellValue((double)liveReport.LicenseeWinLoss);
                row.Cells[ColumnAmountDue].SetCellValue((double)liveReport.AmountDue);
            }

            var currentRow = AssignTotalSummary(sheet, templateSheet);

            if (_data.DataBySport != null)
            {
                // 2 rows empty
                currentRow = AssignBySportsTypeData(sheet, templateSheet, currentRow + SeparateSectionEmptyRow);
            }

            if (_data.DataByCurrency != null)
            {
                // 2 rows empty
                currentRow = AssignByCurrencyData(sheet, templateSheet, currentRow + SeparateSectionEmptyRow);
            }

            if (_data.DataByBetType != null)
            {
                // 2 rows empty
                AssignByBetTypeData(sheet, templateSheet, currentRow + SeparateSectionEmptyRow);
            }
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var reportTitle = string.Format(
                ViewResources.LiveReportTitle,
                GetLiveIndicatorString(_metaModel.LiveIndicator),
                _metaModel.FromDate.ToString(FormatDate),
                _metaModel.ToDate.ToString(FormatDate),
                ExcelHelper.GetTitleCurrencyName(_options));

            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount,
                reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        /// <summary>
        ///     Process UI, hide redundant row and column.
        /// </summary>
        /// <param name="sheet">Excel sheet.</param>
        protected void ProcessUI(ISheet sheet)
        {
            if (!_options.ShowingOption.CustomerCommVisible)
            {
                const int DetailCustomerWlHeader = SummarySectionRowIndex + 2;
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);
                _builder.HideRow(sheet, DetailCustomerWlHeader);
            }

            if (!_options.ShowingOption.BAVisible)
            {
                _builder.HideColumn(sheet, ColumnLicenseeComm);
            }

            if (!_options.ShowingOption.HasAmountDueToColumn)
            {
                _builder.HideColumn(sheet, ColumnAmountDue);
            }
        }

        private static void AssignCommonData(IRow row, LiveReport liveReport)
        {
            row.Cells[ColumnCustomerWl].SetCellValue((double)liveReport.CustomerWinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue((double)liveReport.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)liveReport.CustomerTotal);
            row.Cells[ColumnTurnover].SetCellValue((double)liveReport.TurnOver);
            row.Cells[ColumnBetCount].SetCellValue(liveReport.BetCount);
            row.Cells[ColumnLicenseeComm].SetCellValue((double)liveReport.LicenseeComm);
            row.Cells[ColumnLicenseeWl].SetCellValue((double)liveReport.LicenseeWinLoss);
            row.Cells[ColumnAmountDue].SetCellValue((double)liveReport.AmountDue);
        }

        private static string GetLiveIndicatorString(int liveIndicator)
        {
            string title;

            switch (liveIndicator)
            {
                case LiveStatusTypes.NonLive:
                    title = ViewResources.NoneLive;
                    break;

                case LiveStatusTypes.Live:
                    title = ViewResources.Live;
                    break;

                case LiveStatusTypes.Mix:
                    title = ViewResources.LiveAndNonLive;
                    break;

                default:
                    title = string.Empty;
                    break;
            }

            return title + ViewResources.Data;
        }

        private void AssignByBetTypeData(ISheet sheet, ISheet templateSheet, int currentRow)
        {
            var liveReportSportList = _data.DataByBetType.ToList();

            if (!liveReportSportList.Any())
            {
                return;
            }

            var rowIndex = currentRow;
            AssignCommonHeader(sheet, templateSheet, ViewResources.ByBetType, ViewResources.BetType,
                RowByBetTypeHeaderTemplate, rowIndex);
            rowIndex += SeparateSectionEmptyRow + 1;

            if (!_options.ShowingOption.CustomerCommVisible)
            {
                // If row isn't hide (in case BA visible), then merge cell and set value for header
                var rowHeaderMerge = rowIndex - 2;
                _builder.MergeCells(sheet, rowHeaderMerge, rowHeaderMerge, rowHeaderMerge + 1, ColumnCustomerWl);
            }

            foreach (var liveReportBetType in liveReportSportList)
            {
                var liveReportBetTypeList = liveReportBetType.LiveReports;

                var row = _builder.CopyRow(sheet, templateSheet, RowByBetTypeProductTitle, rowIndex);
                _builder.MergeCols(sheet, ColumnProduct, ColumnAmountDue, rowIndex);
                rowIndex++;

                var productName = liveReportBetType.ProductName.Equals(ViewResources.SB) ?
                    ViewResources.Sportsbook :
                    liveReportBetType.ProductName;

                row.Cells[ColumnProduct].SetCellValue(productName);

                foreach (var liveReportSport in liveReportBetTypeList)
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowContentDetailTemplate, rowIndex);
                    row.Cells[ColumnProduct].SetCellValue(liveReportSport.ProductName);
                    AssignCommonData(row, liveReportSport);
                    rowIndex++;
                }
            }
        }

        private int AssignByCurrencyData(ISheet sheet, ISheet templateSheet, int currentRow)
        {
            var liveReportSportList = _data.DataByCurrency.ToList();

            var rowIndex = currentRow;

            if (liveReportSportList.Any())
            {
                AssignCommonHeader(sheet, templateSheet, ViewResources.ByCurrency, ViewResources.Currency,
                    RowByCurrencyHeaderTemplate, rowIndex);
                rowIndex += SeparateSectionEmptyRow + 1;

                if (!_options.ShowingOption.CustomerCommVisible)
                {
                    // If row isn't hide (in case BA visible), then merge cell and set value for header
                    var rowHeaderMerge = rowIndex - 2;
                    _builder.MergeCells(sheet, rowHeaderMerge, rowHeaderMerge + 1, ColumnCustomerWl, ColumnCustomerWl);
                }

                for (var i = 0; i < liveReportSportList.Count; i++)
                {
                    var liveReportSport = liveReportSportList.ElementAt(i);

                    var row = _builder.CopyRow(sheet, templateSheet, RowContentDetailTemplate, rowIndex + i);

                    row.Cells[ColumnProduct].SetCellValue(liveReportSport.ProductName);

                    AssignCommonData(row, liveReportSport);
                }
            }

            return rowIndex + liveReportSportList.Count;
        }

        private int AssignBySportsTypeData(ISheet sheet, ISheet templateSheet, int currentRow)
        {
            var liveReportSportList = _data.DataBySport.ToList();

            var rowIndex = currentRow;

            if (!liveReportSportList.Any())
            {
                return rowIndex + liveReportSportList.Count;
            }

            AssignCommonHeader(sheet, templateSheet, ViewResources.BySportsType, ViewResources.Sport,
                RowBySportsTypeHeaderTemplate, rowIndex);
            rowIndex += SeparateSectionEmptyRow + 1;

            if (!_options.ShowingOption.CustomerCommVisible)
            {
                // If row isn't hide (in case BA visible), then merge cell and set value for header
                var rowHeaderMerge = rowIndex - 2;
                _builder.MergeCells(sheet, rowHeaderMerge, rowHeaderMerge + 1, ColumnCustomerWl, ColumnCustomerWl);
            }

            for (var i = 0; i < liveReportSportList.Count; i++)
            {
                var liveReportSport = liveReportSportList.ElementAt(i);

                var row = _builder.CopyRow(sheet, templateSheet, RowBySportsTypeContentTemplate, rowIndex + i);

                row.Cells[ColumnProduct].SetCellValue(liveReportSport.ProductName);

                AssignCommonData(row, liveReportSport);
            }

            return rowIndex + liveReportSportList.Count();
        }

        private int AssignTotalSummary(ISheet sheet, ISheet templateSheet)
        {
            var currentRow = RowContentTemplate + _summaryLiveReports.Count;

            var row = _builder.CopyRow(sheet, templateSheet, RowTotalSummaryTemplate, currentRow);

            row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);
            row.Cells[ColumnCustomerWl].SetCellValue((double)_summaryLiveReports.Sum(a => a.CustomerWinLoss));
            row.Cells[ColumnCustomerComm].SetCellValue((double)_summaryLiveReports.Sum(a => a.CustomerComm));
            row.Cells[ColumnCustomerTotal].SetCellValue((double)_summaryLiveReports.Sum(a => a.CustomerTotal));
            row.Cells[ColumnTurnover].SetCellValue((double)_summaryLiveReports.Sum(a => a.TurnOver));
            row.Cells[ColumnBetCount].SetCellValue(_summaryLiveReports.Sum(a => a.BetCount));
            row.Cells[ColumnLicenseeWl].SetCellValue((double)_summaryLiveReports.Sum(a => a.LicenseeWinLoss));
            row.Cells[ColumnLicenseeComm].SetCellValue((double)_summaryLiveReports.Sum(a => a.LicenseeComm));
            row.Cells[ColumnAmountDue].SetCellValue((double)_summaryLiveReports.Sum(a => a.AmountDue));

            return currentRow + 1;
        }

        private void AssignCommonHeader(ISheet sheet, ISheet templateSheet, string groupName, string firstHeader,
                                        int rowTplIdx, int rowIndex)
        {
            var row1Header = new List<string>
            {
                ViewResources.Turnover,
                ViewResources.BetCount,
                ViewResources.LicenseeW_L,
                ViewResources.Comm,
                ViewResources.AmountDueTo
            };

            // group name
            var row = _builder.CopyRow(sheet, templateSheet, rowTplIdx++, rowIndex++);
            row.Cells[ColumnProduct].SetCellValue(groupName);

            // header row 1
            row = _builder.CopyRow(sheet, templateSheet, rowTplIdx++, rowIndex++);
            row.Cells[ColumnProduct].SetCellValue(firstHeader);
            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.CustomerW_L);
            _builder.MergeCells(sheet, rowIndex - 1, rowIndex, ColumnProduct, ColumnProduct);
            _builder.MergeCols(sheet, ColumnCustomerWl, ColumnCustomerTotal, rowIndex - 1);

            var colIndex = ColumnTurnover;

            foreach (var item in row1Header)
            {
                _builder.MergeCells(sheet, rowIndex - 1, rowIndex, colIndex, colIndex);
                row.Cells[colIndex++].SetCellValue(item);
            }

            // header row 2
            row = _builder.CopyRow(sheet, templateSheet, rowTplIdx, rowIndex);
            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.WinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }
    }
}
