namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using NPOI.SS.UserModel;

    public class ExtraPayoutExcel : IExtraPayoutExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnProduct = 5;
        private const int ColumnPromotionType = 6;
        private const int ColumnTurnOver = 7;
        private const int ColumnTotalTransaction = 8;
        private const int ColumnCustomerAmount = 9;
        private const int ColumnLicSharingAmount = 10;

        private const int RowNoInfoTemplate = 8;
        private const int ColumnTemplateCount = 11;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalCurrencyTemplate = 5;
        private const int RowGrantTotalTemplate = 7;

        private readonly IExcelBuilder _builder;

        public ExtraPayoutExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<ExtraPayout> data, ExtraPayoutMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<ExtraPayout> data, ExtraPayoutMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet, metaModel);

            if (data.DataList.Any())
            {
                AssignSheetData(sheet, templateSheet, data);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            HideColumns(sheet, metaModel);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
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
                ViewResources.Product,
                ViewResources.PromotionType,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerAmount,
                ViewResources.LicSharingAmount
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<ExtraPayout> data)
        {
            int index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(++index);
                AssignCellData(row, item, true);
            }

            AssignTotalCurrency(sheet, templateSheet, data, index + RowContentTemplate);
            AssignGrandTotal(sheet, templateSheet, data, index + RowContentTemplate + data.TotalDataList.Count());
        }

        private static void AssignCellData(IRow row, ExtraPayout item, bool hideLicSharingAmountForTip = false)
        {
            row.Cells[ColumnDate].SetCellValue(item.DateString);
            row.Cells[ColumnExternalId].SetCellValue(item.Nickname);
            row.Cells[ColumnSystemId].SetCellValue(item.Username);
            row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
            row.Cells[ColumnProduct].SetCellValue(item.ProductName);
            row.Cells[ColumnPromotionType].SetCellValue(item.PromotionName);
            row.Cells[ColumnTurnOver].SetCellValue((double)item.Turnover);
            row.Cells[ColumnTotalTransaction].SetCellValue(item.BetCount);
            row.Cells[ColumnCustomerAmount].SetCellValue((double)item.Amount);
            if (item.ProductCode == Product.TIP && hideLicSharingAmountForTip)
            {
                row.Cells[ColumnLicSharingAmount].SetCellValue("-");
            }
            else
            {
                row.Cells[ColumnLicSharingAmount].SetCellValue((double)item.SharingAmount);
            }
        }

        private void AssignTotalCurrency(ISheet sheet, ISheet templateSheet, SortableReportResultModel<ExtraPayout> data, int index)
        {
            var beginIndex = index;

            foreach (var item in data.TotalDataList)
            {
                var row = index == beginIndex ? _builder.CopyRow(sheet, templateSheet, RowTotalCurrencyTemplate, index)
                    : _builder.CopyRow(sheet, templateSheet, RowTotalCurrencyTemplate + 1, index);
                AssignCellData(row, item);
                row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
                _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnCurrency, lastCol: ColumnPromotionType);
                index++;
            }

            _builder.MergeCells(sheet, firstRow: beginIndex, lastRow: index - 1, firstCol: ColumnIndex, lastCol: ColumnSystemId);
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<ExtraPayout> data, int index)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

            AssignCellData(row, data.TotalData);

            row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, data.TotalData.CurrencyName));

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnPromotionType);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, ExtraPayoutMetaModel metaModel)
        {
            var displayedCurrency = string.IsNullOrEmpty(metaModel.BaseCurrency) ? metaModel.CurrencyName : metaModel.BaseCurrency.ToUpperInvariant();
            var currencyText = $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            var baseCurrencyText = metaModel.IsConvertAllToBaseCurrency ? $", {ViewResources.BaseCurrency}: {displayedCurrency}" : "";
            var productText = $"{ViewResources.LicenseeW_L}: ({metaModel.ProductCodeList.Replace(",", ", ")})";
            var userNameText = string.IsNullOrEmpty(metaModel.Username) ? "" : $", {ViewResources.Username}: {metaModel.Username}";

            var fromDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownFromDate : metaModel.FromDate;
            var toDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownToDate : metaModel.ToDate;

            var reportTitle = $"{ViewResources.ExtraPayout}, {fromDate:MM/dd/yyyy} -> {toDate:MM/dd/yyyy}, {currencyText}{baseCurrencyText}, {productText}{userNameText}";

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

        private void HideColumns(ISheet sheet, ExtraPayoutMetaModel metaModel)
        {
            if (metaModel.DrillDownLevel == (int)ReportLevel.Main)
            {
                _builder.HideColumn(sheet, ColumnExternalId, ColumnSystemId, ColumnPromotionType);
            }
            else
            {
                _builder.HideColumn(sheet, ColumnDate);
            }
        }
    }
}