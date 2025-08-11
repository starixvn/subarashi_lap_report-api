namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Common.Excel.Interfaces;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Resources;
    using Resources.Constants;

    public class GodOfFortuneWinLossExcel : IGodOfFortuneWinLossExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnTurnover = 5;
        private const int ColumnDiscountValue = 6;
        private const int ColumnTotalTrans = 7;
        private const int ColumnCustomerWl = 8;
        private const int ColumnLicenseeWl = 9;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 10;
        private const int RowTotalNoSolidBorder = 5;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;
        private const int RowGrantTotalTemplate = 6;

        private readonly IExcelBuilder _builder;

        public GodOfFortuneWinLossExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<GodOfFortuneWinLoss> data, GodOfFortuneWinLossMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<GodOfFortuneWinLoss> data, GodOfFortuneWinLossMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet, metaModel);

            if (data.DataList.Any())
            {
                SortMemberData(data.DataList, metaModel.SortBy, metaModel.SortOrder);
                AssignSheetData(sheet, templateSheet, data, metaModel);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            HideColumnsByLevel(sheet, metaModel);

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private static void SortMemberData(ISortableList<GodOfFortuneWinLoss> dataList, string sortBy, string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                var property = typeof(GodOfFortuneWinLoss).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    dataList.Sort(property.Name, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(GodOfFortuneWinLoss.Index), true);
                }
            }
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);

            var headerTextList = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Currency,
                ViewResources.Turnover,
                ViewResources.DiscountValue,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<GodOfFortuneWinLoss> data, GodOfFortuneWinLossMetaModel metaModel)
        {
            var rowSpan = 1;
            var rowIndex = 1;
            var indexDetail = 1;
            int index;
            var rowCount = data.DataList.Count();

            for (index = 0; index < rowCount; index++)
            {
                var item = data.DataList[index];

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);

                var reportDateString = metaModel.ReportBy == ReportByType.Monthly
                 ? item.Date.ToString("MM/yyyy")
                 : item.Date.ToShortDateString();

                // Merge first two columns
                var needMerge = index >= rowCount - 1 ||
                                 item.Date.Date != data.DataList[index + 1].Date.Date;

                // Merge cell
                if (needMerge && rowSpan > 1)
                {
                    MergeCells(sheet, rowSpan, index, metaModel);
                }

                rowSpan = needMerge ? 1 : rowSpan + 1;

                var indexid = metaModel.DrillDownLevel == 0 ? rowIndex : indexDetail++;

                row.Cells[ColumnIndex].SetCellValue(indexid);
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
                row.Cells[ColumnDate].SetCellValue(reportDateString);

                AssignCellData(row, item);

                rowIndex = needMerge ? rowIndex + 1 : rowIndex;
            }

            index += RowContentTemplate;

            AssignTotal(sheet, templateSheet, data, ref index);
            AssignGrandTotal(sheet, templateSheet, data, index);
        }

        private static void AssignCellData(IRow row, GodOfFortuneWinLoss item)
        {
            row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
            row.Cells[ColumnTurnover].SetCellValue((double)item.TurnOver);
            row.Cells[ColumnDiscountValue].SetCellValue((double)item.Discount);
            row.Cells[ColumnTotalTrans].SetCellValue(item.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue((double)item.CustomerWinLoss);
            row.Cells[ColumnLicenseeWl].SetCellValue((double)item.LicenseeWinLoss);
        }

        private void AssignTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<GodOfFortuneWinLoss> data, ref int index)
        {
            int beginTotalRow = index;

            foreach (GodOfFortuneWinLoss totalCurrency in data.TotalDataList)
            {
                IRow row;

                if (index == beginTotalRow)
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, index);
                    row.Cells[0].SetCellValue(ViewResources.Total);
                }
                else
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalNoSolidBorder, index);
                }

                row.Cells[ColumnCurrency].SetCellValue(totalCurrency.CurrencyName);

                AssignCellData(row, totalCurrency);

                index++;
            }

            // Merge first two column
            const int LastTotalMergedCol = 3;

            _builder.MergeCells(sheet, firstRow: beginTotalRow, lastRow: index - 1, firstCol: ColumnIndex, lastCol: LastTotalMergedCol);
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<GodOfFortuneWinLoss> data, int index)
        {
            var currencies = data.SummaryTotal.Keys.Reverse();

            foreach (string currency in currencies)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

                AssignCellData(row, data.SummaryTotal[currency]);

                row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, currency));

                // Merge first two column
                _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnCurrency);
                index++;
            }
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, GodOfFortuneWinLossMetaModel metaModel)
        {
            var baseCurrencyText = string.Format(
                ViewResources.Viewin,
                !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName);

            var isMainLevel = metaModel.DrillDownLevel == (int)ReportLevel.Main;
            var fromdate = isMainLevel ? metaModel.FromDate : metaModel.DrillDownFromDate;
            var todate = isMainLevel ? metaModel.ToDate : metaModel.DrillDownToDate;

            var reportTitle = string.Format(
                                         ViewResources.GodOfFortuneWinLossTitle,
                                         fromdate.ToString("MM/dd/yyyy"),
                                         todate.ToString("MM/dd/yyyy"),
                                         baseCurrencyText);

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

        private void HideColumnsByLevel(ISheet sheet, GodOfFortuneWinLossMetaModel metaModel)
        {
            switch (metaModel.DrillDownLevel)
            {
                case (int)ReportLevel.Main:
                    _builder.HideColumn(sheet, ColumnExternalId, ColumnSystemId);
                    break;

                case (int)ReportLevel.Licensee:
                    _builder.HideColumn(sheet, ColumnExternalId, ColumnDate);
                    break;

                default:
                    _builder.HideColumn(sheet, ColumnDate);
                    break;
            }
        }

        private void MergeCells(ISheet sheet, int rowSpan, int index, GodOfFortuneWinLossMetaModel metaModel)
        {
            var mergedRowIndex = metaModel.DrillDownLevel == (int)ReportLevel.Main ? ColumnIndex : ColumnDate;
            var beginRow = index + RowContentTemplate - rowSpan + 1;
            var endRow = index + RowContentTemplate;

            _builder.MergeCells(sheet, firstRow: beginRow, lastRow: endRow, firstCol: mergedRowIndex, lastCol: mergedRowIndex);
        }
    }
}