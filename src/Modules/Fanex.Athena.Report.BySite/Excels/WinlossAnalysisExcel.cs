namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Excel.Interfaces;
    using DTOs;
    using DTOs.WinlossAnalysis;
    using Enums;
    using Interfaces;
    using Models;
    using NPOI.HSSF.Util;
    using NPOI.SS.UserModel;
    using Options;
    using Resources;

    public class WinlossAnalysisExcel : IWinlossAnalysisExcel
    {
        private const string DATE_FORMAT = "MM/dd/yyyy";
        private const string TEMPLATE_SHEET_NAME = "Template";
        private const string SHEET_NAME = "Data";
        private const int GROUP_COL_CELL_RANGE = 2;
        private const int MAX_NUMBER_OF_COL = 14;
        private const int TEMPLATE_TITLE_ROW = 0;
        private const int TEMPLATE_HEADER_ROW = 1;
        private const int TEMPLATE_GROUP_ROW = 3;
        private const int TEMPLATE_DETAIL_ROW = 4;
        private const int TEMPLATE_TOTAL_ROW = 9;

        private readonly IDictionary<string, ICellStyle> _negativeCellStyleDic = new Dictionary<string, ICellStyle>();
        private readonly IDictionary<string, IFont> _negativeCellFontDic = new Dictionary<string, IFont>();

        private const int TemplateRowIndexCol = 0;
        private const int TemplateGroupByCol = 1;
        private const int TemplateBreakdownCol = 3;
        private const int TemplateBetCountCol = 5;
        private const int TemplateTurnoverCol = 6;
        private const int TemplateNetTurnoverCol = 7;
        private const int TemplatePlayerWinlossCol = 8;
        private const int TemplateBetEffectiveCol = 9;
        private const int TemplatePlayerCommCol = 10;
        private const int TemplateLicenseeWinlossCol = 11;
        private const int TemplateLicenseeComm = 12;
        private const int TemplateMarginCol = 13;

        private WinlossAnalysisExcelOption Option { get; set; } = new WinlossAnalysisExcelOption();

        private bool IsHasSecondaryHeaderForGroupBy => Option.GroupByMode == GroupByMode.Player;

        private bool IsHasSecondaryHeaderForBreakDown => Option.BreakdownMode == BreakdownMode.Player;

        private bool IsHasBetEffectiveCol => Option.GroupByMode == GroupByMode.Player;

        private bool IsHasSecondaryHeader => IsHasSecondaryHeaderForGroupBy || IsHasSecondaryHeaderForBreakDown;

        private readonly IExcelBuilder _builder;
        private readonly IColor _redColor = new HSSFColor.Red();

        public WinlossAnalysisExcel(IExcelBuilder builder)
            => _builder = builder;

        public byte[] Export(string templatePath, WinlossAnalysisResultModel data, WinlossAnalysisExcelOption option)
        {
            Option = option;
            return ExportToExcelFileToMemory(templatePath, data);
        }

        private byte[] ExportToExcelFileToMemory(string templatePath, WinlossAnalysisResultModel data)
        {
            var workBook = _builder.LoadTemplate(templatePath);
            FillContentToExcel(workBook, data);
            _builder.RemoveSheet(workBook, TEMPLATE_SHEET_NAME);
            return ExportToMemory(workBook);
        }

        private void FillContentToExcel(IWorkbook workBook, WinlossAnalysisResultModel data)
        {
            var templateSheet = workBook.GetSheet(TEMPLATE_SHEET_NAME);
            var sheet = workBook.CreateSheet(SHEET_NAME);
            InitColumnWidth(templateSheet, sheet);
            FillTitle(templateSheet, sheet);
            FillAllHeader(templateSheet, sheet);
            FillAllData(templateSheet, sheet, data);
            RemoveReduntdantRowAndCol(sheet);
            FreezeHeader(sheet);
        }

        private static void InitColumnWidth(ISheet templateSheet, ISheet destSheet)
        {
            for (var i = 0; i < MAX_NUMBER_OF_COL; i++)
            {
                destSheet.SetColumnWidth(i, templateSheet.GetColumnWidth(i));
            }
        }

        private void FillTitle(ISheet templateSheet, ISheet destSheet)
        {
            _builder.SetRowValue(destSheet, templateSheet, TEMPLATE_TITLE_ROW, TEMPLATE_TITLE_ROW, MAX_NUMBER_OF_COL, GetTitle());
        }

        private void FillAllHeader(ISheet templateSheet, ISheet destSheet)
        {
            SetUpMergeHeaderRegion(destSheet);
            var templateRowHeight = templateSheet.GetRow(TEMPLATE_HEADER_ROW).Height;
            var headerRow1 = _builder.CopyRow(destSheet, templateSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW);
            var headerRow2 = _builder.CopyRow(destSheet, templateSheet, TEMPLATE_HEADER_ROW + 1, TEMPLATE_HEADER_ROW + 1);
            headerRow1.Height = headerRow2.Height = templateRowHeight;

            var commonHeaderName = new[]
            {
                ViewResources.BetCount,
                ViewResources.Turnover,
                ViewResources.NetTurnover,
                ViewResources.PlayerW_L,
                ViewResources.EffectiveBettingValue,
                ViewResources.PlayerComm,
                ViewResources.LicenseeW_L,
                ViewResources.LicenseeComm,
                ViewResources.Margin
            };
            headerRow1.GetCell(TemplateGroupByCol).SetCellValue(ViewResources.GroupBy);
            headerRow1.GetCell(TemplateBreakdownCol).SetCellValue(ViewResources.BreakdownBy);
            var currentCommonHeaderColIndex = TemplateBetCountCol;
            for (int i = 0; i < commonHeaderName.Count(); i++)
            {
                headerRow1.GetCell(currentCommonHeaderColIndex).SetCellValue(commonHeaderName[i]);
                currentCommonHeaderColIndex++;
            }

            if (IsHasSecondaryHeader)
            {
                headerRow2.GetCell(TemplateGroupByCol).SetCellValue(ViewResources.SystemId);
                headerRow2.GetCell(TemplateGroupByCol + 1).SetCellValue(ViewResources.ExternalId);
                headerRow2.GetCell(TemplateBreakdownCol).SetCellValue(ViewResources.SystemId);
                headerRow2.GetCell(TemplateBreakdownCol + 1).SetCellValue(ViewResources.ExternalId);
            }
        }

        private void SetUpMergeHeaderRegion(ISheet destSheet)
        {
            // Merge RowIndex Header
            MergeCells(destSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW + 1, TemplateRowIndexCol, TemplateRowIndexCol);
            // Merge GroupBy Header
            MergeCells(destSheet,
                TEMPLATE_HEADER_ROW,
                IsHasSecondaryHeaderForGroupBy ? TEMPLATE_HEADER_ROW : TEMPLATE_HEADER_ROW + 1,
                TemplateGroupByCol,
                TemplateGroupByCol + 1);

            // Merge Breakdown Header
            MergeCells(destSheet,
                TEMPLATE_HEADER_ROW,
                IsHasSecondaryHeaderForBreakDown ? TEMPLATE_HEADER_ROW : TEMPLATE_HEADER_ROW + 1,
                TemplateBreakdownCol,
                TemplateBreakdownCol + 1);

            // Merge remaining Header
            for (int i = TemplateBetCountCol; i < MAX_NUMBER_OF_COL; i++)
            {
                MergeCells(destSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW + 1, i, i);
            }
        }

        private void FillAllData(ISheet templateSheet, ISheet sheet, WinlossAnalysisResultModel data)
        {
            var mainData = data.Data;
            var totalGroupByRow = mainData.Count();
            var totalDetailRow = Option.ExportExcelMode == WinlossAnalysisExportMode.All ? mainData.Select(_ => _.Children.Count()).Sum() : 0;
            var sumTotalRowIndex = TEMPLATE_GROUP_ROW + totalGroupByRow + totalDetailRow;
            AssignMainData(templateSheet, sheet, data.Data);
            AssginSumTotalData(templateSheet, sheet, data.SumTotal, sumTotalRowIndex);
        }

        private void AssignMainData(ISheet templateSheet, ISheet sheet, IEnumerable<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>> datas)
        {
            var currenRowIndex = TEMPLATE_GROUP_ROW;
            var templateGroupRowHeight = templateSheet.GetRow(TEMPLATE_GROUP_ROW).Height;
            var templateDetailRowHeight = templateSheet.GetRow(TEMPLATE_DETAIL_ROW).Height;
            foreach (var data in datas)
            {
                var groupData = data.KeyItem;
                var groupRow = _builder.CopyRow(sheet, templateSheet, TEMPLATE_GROUP_ROW, currenRowIndex);
                groupRow.Height = templateGroupRowHeight;
                groupRow.GetCell(TemplateRowIndexCol).SetCellValue(groupData.DisplayRowIndex);
                AssignDataForNormalRow(groupRow, groupData, groupData);
                currenRowIndex++;
                if (Option.ExportExcelMode == WinlossAnalysisExportMode.All)
                {
                    var groupIndex = currenRowIndex;
                    foreach (var detailData in data.Children)
                    {
                        var detailRow = _builder.CopyRow(sheet, templateSheet, TEMPLATE_DETAIL_ROW, currenRowIndex);
                        detailRow.Height = templateDetailRowHeight;
                        AssignDataForNormalRow(detailRow, detailData, groupData);
                        currenRowIndex++;
                    }
                    sheet.GroupRow(groupIndex, groupIndex + data.Children.Count());
                    sheet.SetRowGroupCollapsed(groupIndex, true);
                }
            }
        }

        private void AssignDataForNormalRow(IRow row, WinlossAnalysisDTO item, WinlossAnalysisDTO parentData)
        {
            row.GetCell(TemplateGroupByCol).SetCellValue(parentData.GroupByFieldText);
            row.GetCell(TemplateBreakdownCol).SetCellValue(item.BreakDownFieldText);
            if (IsHasSecondaryHeaderForGroupBy)
            {
                row.GetCell(TemplateGroupByCol + 1).SetCellValue(parentData.ExternalId);
            }
            if (IsHasSecondaryHeaderForBreakDown)
            {
                row.GetCell(TemplateBreakdownCol + 1).SetCellValue(item.ExternalId);
            }
            AssignCommonData(row, item);
        }

        private void AssginSumTotalData(ISheet templateSheet, ISheet sheet, WinlossAnalysisDTO sumTotal, int sumTotalRowIndex)
        {
            _builder.MergeCells(sheet, sumTotalRowIndex, sumTotalRowIndex, TemplateRowIndexCol, TemplateBreakdownCol + 1);
            var totalRow = _builder.CopyRow(sheet, templateSheet, TEMPLATE_TOTAL_ROW, sumTotalRowIndex);
            totalRow.GetCell(0).SetCellValue(ViewResources.Total);
            AssignCommonData(totalRow, sumTotal);
        }

        private void AssignCommonData(IRow row, WinlossAnalysisDTO item)
        {
            row.GetCell(TemplateBetCountCol).SetCellValue(item.BetCount);
            row.GetCell(TemplateTurnoverCol).SetCellValue((double)item.Turnover);
            row.GetCell(TemplateNetTurnoverCol).SetCellValue((double)item.NetTurnover);
            row.GetCell(TemplatePlayerWinlossCol).SetCellValue((double)item.PlayerWinloss);
            row.GetCell(TemplateBetEffectiveCol).SetCellValue((double)item.BetEffective);
            row.GetCell(TemplatePlayerCommCol).SetCellValue((double)item.PlayerComm);
            row.GetCell(TemplateLicenseeWinlossCol).SetCellValue((double)item.LicWinloss);
            row.GetCell(TemplateLicenseeComm).SetCellValue((double)item.LicComm);
            row.GetCell(TemplateMarginCol).SetCellValue(item.Margin);

            if (item.LicWinloss < 0)
            {
                SetStyleForNegativeValue(row.GetCell(TemplateLicenseeWinlossCol));
            }
            if (item.Margin < 0)
            {
                SetStyleForNegativeValue(row.GetCell(TemplateMarginCol));
            }
        }

        private void SetStyleForNegativeValue(ICell cell)
        {
            cell.CellStyle = GetNegativeCellStyle(cell);
        }

        private ICellStyle GetNegativeCellStyle(ICell cell)
        {
            var cellStyle = cell.CellStyle;
            var workbook = cell.Row.Sheet.Workbook;
            var key = GenerateKeyForStyle(workbook, cellStyle.Index);
            if (_negativeCellStyleDic.ContainsKey(key))
            {
                return _negativeCellStyleDic[key];
            }
            var newCellStyle = workbook.CreateCellStyle();
            newCellStyle.CloneStyleFrom(cellStyle);
            var font = GetNegativeValueFont(cell);
            newCellStyle.SetFont(font);
            _negativeCellStyleDic.Add(key, newCellStyle);
            return newCellStyle;
        }

        private IFont GetNegativeValueFont(ICell cell)
        {
            var workbook = cell.Row.Sheet.Workbook;
            var font = cell.CellStyle.GetFont(workbook);
            var key = GenerateKeyForStyle(workbook, font.Index);
            if (_negativeCellFontDic.ContainsKey(key))
            {
                return _negativeCellFontDic[key];
            }
            var newFont = workbook.CreateFont();
            newFont.CloneStyleFrom(font);
            newFont.Color = _redColor.Indexed;
            _negativeCellFontDic.Add(key, newFont);
            return newFont;
        }

        private string GetTitle()
            => $"{ViewResources.WinlossAnalysis}" +
                $" - {(Option.ViewInTime == ViewInTimeMode.Settlement ? ViewResources.SettlementTime : ViewResources.FinancialDate)}," +
                $" {Option.FromDate.ToString(DATE_FORMAT)} → {Option.ToDate.ToString(DATE_FORMAT)}," +
                $" {ViewResources.Currency}: {Option.CurrencyList}," +
                $" {ViewResources.BaseCurrency}: {Option.BaseCurrency}," +
                $" {ViewResources.LicenseeW_L} ({Option.ProductCodes})" +
                (string.IsNullOrWhiteSpace(Option.SearchUsername) ? string.Empty : $", {ViewResources.Username}: {Option.SearchUsername}");

        private static void FreezeHeader(ISheet sheet)
        {
            sheet.CreateFreezePane(0, TEMPLATE_HEADER_ROW + GROUP_COL_CELL_RANGE);
        }

        private static byte[] ExportToMemory(IWorkbook workBook)
        {
            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();
                return stream.ToArray();
            }
        }

        private void RemoveReduntdantRowAndCol(ISheet sheet)
        {
            if (!IsHasSecondaryHeader)
            {
                const int secondaryRowIndex = TEMPLATE_HEADER_ROW + 1;
                _builder.HideRow(sheet, secondaryRowIndex);
            }
            if (!IsHasSecondaryHeaderForGroupBy)
            {
                _builder.HideColumn(sheet, TemplateGroupByCol + 1);
            }
            if (!IsHasSecondaryHeaderForBreakDown)
            {
                _builder.HideColumn(sheet, TemplateBreakdownCol + 1);
            }
            if (!IsHasBetEffectiveCol)
            {
                _builder.HideColumn(sheet, TemplateBetEffectiveCol);
            }
            if (Option.ExportExcelMode == WinlossAnalysisExportMode.GroupByModeOnly)
            {
                _builder.HideColumn(sheet, TemplateBreakdownCol);
            }
        }

        private void MergeCells(ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
            => _builder.MergeCells(sheet, firstRow, lastRow, firstCol, lastCol);

        private static string GenerateKeyForStyle(IWorkbook workbook, short index)
            => $"{workbook.GetHashCode()}_{index}";
    }
}