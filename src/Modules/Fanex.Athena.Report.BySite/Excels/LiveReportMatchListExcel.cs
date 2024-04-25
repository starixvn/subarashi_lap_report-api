using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Resources;
using NPOI.SS.UserModel;
using static Fanex.Athena.Report.BySite.Constants.ReportConstant;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class LiveReportMatchListExcel : ILiveReportMatchListExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        public const string FormatDate = "MM/dd/yyyy";

        private const int ColumnBetCount = 5;

        private const int ColumnCustomerComm = 2;

        private const int ColumnCustomerTotal = 3;

        private const int ColumnCustomerWl = 1;

        private const int ColumnLicenseeComm = 7;

        private const int ColumnAmountDue = 8;

        private const int ColumnLicenseeWl = 6;

        private const int ColumnMatch = 0;

        private const int ColumnTemplateCount = 9;

        private const int ColumnTurnover = 4;

        private const int RowSportType = 1;

        private const int RowContentTemplate = 4;

        private const int RowNoInfoTemplate = 5;

        private const int RowTableHeaderTemplate = 2;

        public const int RowTitleTemplate = 0;

        private readonly IExcelBuilder _builder;

        private readonly string SportName;

        private SortableList<LiveReportWinLossByMatch> _data;

        private LiveReportMatchListMetalModel _metaModel;

        private LiveReportExcelOption _options;

        public LiveReportMatchListExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableList<LiveReportWinLossByMatch> data,
                             LiveReportMatchListMetalModel metaModel,
                             LiveReportExcelOption excelOption)
        {
            _data = data;
            _metaModel = metaModel;
            _options = excelOption;

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

            if (_data.Any())
            {
                AssignSheetData(sheet, templateSheet);
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
            var rowTplIdx = 2;
            var rowIndex = 2;

            var row1Header = new List<string>
            {
                ViewResources.Turnover,
                ViewResources.BetCount,
                ViewResources.LicenseeW_L,
                ViewResources.Comm,
                ViewResources.AmountDueTo
            };

            // header row 1
            var row = _builder.CopyRow(sheet, templateSheet, rowTplIdx++, rowIndex++);
            row.Cells[ColumnMatch].SetCellValue(ViewResources.Match);
            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.CustomerW_L);
            _builder.MergeCells(sheet, rowIndex - 1, rowIndex, ColumnMatch, ColumnMatch);
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

        protected void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowSportType, RowSportType, 1, SportName);
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount,
                ViewResources.NoInfo);
            ProcessUi(sheet);
        }

        protected void AssignSheetData(ISheet sheet, ISheet templateSheet)
        {
            var rowCount = _data.Count;

            _builder.SetRowValue(sheet, templateSheet, RowSportType, RowSportType, 1, SportName);

            for (var index = 0; index < rowCount; index++)
            {
                var dataItem = _data[index];

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);

                AssignCommonData(row, dataItem);
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

            sheet.CreateFreezePane(0, RowContentTemplate);

            _builder.ClearTemplate(sheet, RowContentTemplate);
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

        protected void ProcessUi(ISheet sheet)
        {
            if (!_options.ShowingOption.CustomerCommVisible)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);
                _builder.HideColumn(sheet, RowTableHeaderTemplate + 1);
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

        private static void AssignCommonData(IRow row, LiveReportWinLossByMatch dataItem)
        {
            row.Cells[ColumnMatch].SetCellValue(dataItem.MatchName);
            row.Cells[ColumnCustomerWl].SetCellValue((double)dataItem.CustomerWinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue((double)dataItem.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)dataItem.CustomerTotal);
            row.Cells[ColumnTurnover].SetCellValue((double)dataItem.TurnOver);
            row.Cells[ColumnBetCount].SetCellValue(dataItem.BetCount);
            row.Cells[ColumnLicenseeComm].SetCellValue((double)dataItem.LicenseeComm);
            row.Cells[ColumnLicenseeWl].SetCellValue((double)dataItem.LicenseeWinLoss);
            row.Cells[ColumnAmountDue].SetCellValue((double)dataItem.AmountDue);
        }
    }
}
