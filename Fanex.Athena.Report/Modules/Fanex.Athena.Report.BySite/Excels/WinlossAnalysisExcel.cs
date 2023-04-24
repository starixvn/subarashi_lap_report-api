using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using NPOI.HSSF.Util;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class WinlossAnalysisExcel : BaseExcel, IWinlossAnalysisExcel, IExportZipFile<WinlossAnalysisResultModel>
    {
        private const string DATE_FORMAT = "dd/MM/yyyy";
        private const string TEMPLATE_SHEET_NAME = "Template";
        private const string SHEET_NAME = "Data";
        private const int MAX_NUMBER_OF_COL = 15;
        private const int TEMPLATE_TITLE_ROW = 0;
        private const int TEMPLATE_HEADER_ROW = 1;
        private const int TEMPLATE_GROUP_ROW = 3;
        private const int TEMPLATE_DETAIL_ROW = 4;
        private const int TEMPLATE_TOTAL_ROW = 9;

        private readonly IDictionary<short, ICellStyle> _negativeCellStyleDic = new Dictionary<short, ICellStyle>();
        private Lazy<IFont> _negativeCellFont = new Lazy<IFont>();

        private const int TemplateRowIndexCol = 0;
        private int TemplateGroupByCol => TemplateRowIndexCol + 1;
        private int TemplateBreakdownCol => TemplateGroupByCol + 2;
        private int TemplateBetCountCol => TemplateBreakdownCol + 2 ;
        private int TemplateTurnoverCol => TemplateBetCountCol + 1;
        private int TemplateNetTurnoverCol => TemplateTurnoverCol + 1;
        private int TemplatePlayerWinlossCol => TemplateNetTurnoverCol + 1;
        private int TemplateBetEffectiveCol => TemplatePlayerWinlossCol + 1;
        private int TemplatePlayerCommCol => TemplateBetEffectiveCol + 1;
        private int TemplateLicenseeWinlossCol => TemplatePlayerCommCol + 1;
        private int TemplateLicenseeComm => TemplateLicenseeWinlossCol + 1;
        private int TemplateAmtDueToBACol => TemplateLicenseeComm + 1;
        private int TemplateMarginCol => TemplateAmtDueToBACol + 1;

        private WinlossAnalysisExcelOption Option { get; set; } = new WinlossAnalysisExcelOption();

        private bool IsHasSecondaryHeaderForGroupBy => Option.GroupByMode == Enums.GroupByMode.Player;
        private bool IsHasSecondaryHeaderForBreakDown => Option.BreakdownMode == Enums.BreakdownMode.Player;
        private bool IsHasBACol => Option.IsHasBAProduct;
        private bool IsHasBetEffectiveCol => Option.GroupByMode == Enums.GroupByMode.Player;
        private bool IsHasSecondaryHeader => IsHasSecondaryHeaderForGroupBy || IsHasSecondaryHeaderForBreakDown;

        private readonly IExcelBuilder _builder;
        private readonly IColor _redColor = new HSSFColor.Red();

        public WinlossAnalysisExcel(IExcelBuilder builder,
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IWebHostEnvironment environment) : base(environment, fileService, excelOptionsAccessor)
        {
            _builder = builder;
        }

        public byte[] Export(string templatePath, WinlossAnalysisResultModel data, WinlossAnalysisExcelOption option)
        {
            Option = option;
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
            for(var i = 0; i < MAX_NUMBER_OF_COL; i++)
            {
                destSheet.SetColumnWidth(i, templateSheet.GetColumnWidth(i));
            }
        }

        private void FillTitle(ISheet templateSheet, ISheet destSheet)
        {
            _builder.SetRowValue(destSheet, templateSheet, TEMPLATE_TITLE_ROW, TEMPLATE_TITLE_ROW, MAX_NUMBER_OF_COL, GetTitle());
            destSheet.GetRow(TEMPLATE_TITLE_ROW).Height = templateSheet.GetRow(TEMPLATE_TITLE_ROW).Height;
        }

        private void FillAllHeader(ISheet templateSheet, ISheet destSheet)
        {            
            SetUpMergeHeaderRegion(destSheet);
            var templateRowHeight = templateSheet.GetRow(TEMPLATE_HEADER_ROW).Height;
            var headerRow1 = _builder.CopyRow(destSheet, templateSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW);
            var headerRow2 = _builder.CopyRow(destSheet, templateSheet, TEMPLATE_HEADER_ROW + 1, TEMPLATE_HEADER_ROW + 1);
            headerRow1.Height = headerRow2.Height = templateRowHeight;

            var commonHeaderName = new []
            {
                ViewResources.BetCount,
                ViewResources.Turnover,
                ViewResources.NetTurnover,
                ViewResources.PlayerW_L,
                ViewResources.EffectiveBettingValue,
                ViewResources.PlayerComm,
                ViewResources.LicenseeW_L,
                ViewResources.LicenseeComm,
                ViewResources.AmountDueToBA,
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
            if(IsHasSecondaryHeaderForGroupBy)
            {
                MergeCells(destSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW, TemplateGroupByCol, TemplateGroupByCol + 1);
            }
            else
            {
                MergeCells(destSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW + 1, TemplateGroupByCol, TemplateGroupByCol + 1);
            }
            // Merge Breakdown Header
            if (IsHasSecondaryHeaderForBreakDown)
            {
                MergeCells(destSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW, TemplateBreakdownCol, TemplateBreakdownCol + 1);
            }
            else
            {
                MergeCells(destSheet, TEMPLATE_HEADER_ROW, TEMPLATE_HEADER_ROW + 1, TemplateBreakdownCol, TemplateBreakdownCol + 1);
            }
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
            var totalDetailRow = mainData.Select(_ => _.Children.Count()).Sum();
            var sumTotalRowIndex = TEMPLATE_GROUP_ROW + totalGroupByRow + totalDetailRow;
            AssignMainData(templateSheet, sheet, data.Data);
            AssginSumTotalData(templateSheet, sheet, data.SumTotal, sumTotalRowIndex);
        }

        private void AssignMainData(ISheet templateSheet, ISheet sheet, IEnumerable<TreeViewItem<WinlossAnalysisViewModel, WinlossAnalysisViewModel>> datas)
        {
            var currenRowIndex = TEMPLATE_GROUP_ROW;
            var templateGroupRowHeight = templateSheet.GetRow(TEMPLATE_GROUP_ROW).Height;
            var templateDetailRowHeight = templateSheet.GetRow(TEMPLATE_DETAIL_ROW).Height;
            foreach (var data in datas)
            {
                var groupData = data.Item;
                var groupRow = _builder.CopyRow(sheet, templateSheet, TEMPLATE_GROUP_ROW, currenRowIndex);
                groupRow.Height = templateGroupRowHeight;
                groupRow.GetCell(TemplateRowIndexCol).SetCellValue(groupData.RowIndex);
                AssignDataForNormalRow(groupRow, groupData, groupData);
                currenRowIndex++;
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

        private void AssignDataForNormalRow(IRow row, WinlossAnalysisViewModel item, WinlossAnalysisViewModel parentData)
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

        private void AssginSumTotalData(ISheet templateSheet, ISheet sheet, WinlossAnalysisViewModel sumTotal, int sumTotalRowIndex)
        {
            _builder.MergeCells(sheet, sumTotalRowIndex, sumTotalRowIndex, TemplateRowIndexCol, TemplateBreakdownCol + 1);
            var totalRow = _builder.CopyRow(sheet, templateSheet, TEMPLATE_TOTAL_ROW, sumTotalRowIndex);
            totalRow.GetCell(0).SetCellValue(ViewResources.Total);
            AssignCommonData(totalRow, sumTotal);
        }

        private void AssignCommonData(IRow row, WinlossAnalysisViewModel item)
        {
            row.GetCell(TemplateBetCountCol).SetCellValue(item.BetCount);
            row.GetCell(TemplateTurnoverCol).SetCellValue((double) item.Turnover);
            row.GetCell(TemplateNetTurnoverCol).SetCellValue((double) item.NetTurnover);
            row.GetCell(TemplatePlayerWinlossCol).SetCellValue((double) item.PlayerWinloss);
            row.GetCell(TemplateBetEffectiveCol).SetCellValue((double) item.BetEffective);
            row.GetCell(TemplatePlayerCommCol).SetCellValue((double) item.PlayerComm);
            row.GetCell(TemplateLicenseeWinlossCol).SetCellValue((double) item.LicWinloss);
            row.GetCell(TemplateLicenseeComm).SetCellValue((double)item.LicComm);
            row.GetCell(TemplateAmtDueToBACol).SetCellValue((double)item.AmtDueToBA);
            row.GetCell(TemplateMarginCol).SetCellValue(item.Margin);

            if (item.LicWinloss < 0)
            {
                SetStyleForNegativeValue(row.GetCell(TemplateLicenseeWinlossCol));
            }
            if(item.Margin < 0)
            {
                SetStyleForNegativeValue(row.GetCell(TemplateMarginCol));
            }
        }

        private void SetStyleForNegativeValue(ICell cell)
        {
            cell.CellStyle= GetNegativeCellStyle(cell);
        }

        private ICellStyle GetNegativeCellStyle(ICell cell)
        {            
            var cellStyle = cell.CellStyle;
            if (_negativeCellStyleDic.ContainsKey(cellStyle.Index))
            {
                return _negativeCellStyleDic[cellStyle.Index];
            }
            var workbook = cell.Row.Sheet.Workbook;
            var newCellStyle = workbook.CreateCellStyle();
            newCellStyle.CloneStyleFrom(cellStyle);
            var font = GetNegativeValueFont(cell);
            newCellStyle.SetFont(font);
            _negativeCellStyleDic.Add(cellStyle.Index, newCellStyle);
            return newCellStyle;
        }

        private IFont GetNegativeValueFont(ICell cell)
        {
            if (_negativeCellFont.IsValueCreated)
            {
                return _negativeCellFont.Value;
            }
            _negativeCellFont = new Lazy<IFont>(() =>
            {
                var workbook = cell.Row.Sheet.Workbook;
                var font = workbook.CreateFont();
                font.CloneStyleFrom(cell.CellStyle.GetFont(cell.Row.Sheet.Workbook));
                font.Color = _redColor.Indexed;
                return font;
            });
            return _negativeCellFont.Value;
        }

        private string GetTitle()       
            => $"{ViewResources.WinlossAnalysis}," +
                $" {Option.FromDate.ToString(DATE_FORMAT)} -> {Option.ToDate.ToString(DATE_FORMAT)}," +
                $" {ViewResources.Currency}: {Option.CurrencyList}," +
                $" {ViewResources.BaseCurrency}: {Option.BaseCurrency}," +
                $" {ViewResources.LicenseeW_L}: {Option.ProductCodes}";

        private static void FreezeHeader(ISheet sheet)
        {
            sheet.CreateFreezePane(0, TEMPLATE_HEADER_ROW + 2);
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
            if (!IsHasBACol)
            {
                _builder.HideColumn(sheet, TemplateAmtDueToBACol);
            }
            if (!IsHasBetEffectiveCol)
            {
                _builder.HideColumn(sheet, TemplateBetEffectiveCol);
            }
        }

        private void MergeCells(ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
            => _builder.MergeCells(sheet, firstRow, lastRow, firstCol, lastCol);

        public Task<string> CreateZipFile(WinlossAnalysisResultModel metaModel)
        {
            throw new NotImplementedException();
        }
    }
}
