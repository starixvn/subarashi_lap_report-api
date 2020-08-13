namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using NPOI.SS.UserModel;

    public class PromotionPrizeExcel : IPromotionPrizeExcel
    {
        private const string DefaultTemplateSheetName = "Template";
        private const string PPCTemplateSheetName = "PPCTemplate";
        private const string DefaultSheetName = "Data";
        public const string FormatDate = "MM/dd/yyyy";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnPromotionType = 2;
        private const int ColumnCurrency = 3;
        private const int ColumnExternalId = 4;
        private const int ColumnSystemId = 5;
        private const int ColumnAmount = 6;
        private const int ColumnTurnover = 6;
        private const int ColumnContributionAmount = 7;

        private const int ColumnTemplateCount = 7;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 2;
        private const int RowTotal = 4;
        private const int RowGrandTotal = 6;
        private const int RowNoInfoTemplate = 7;

        private readonly IExcelBuilder _builder;

        private string _title;

        private static readonly Dictionary<string, Tuple<string, string>> ProductOrderMap =
            new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
                {Product.PP,new Tuple<string, string>("", DefaultTemplateSheetName)  },
                {Product.PPC,new Tuple<string, string>("", PPCTemplateSheetName)},
                {Product.SPR, new Tuple<string, string>(" - " + @ViewResources.PromotionPrize, DefaultTemplateSheetName) }
        };

        public PromotionPrizeExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public IWorkbook Export(string template, IDictionary<string, PromotionPrizeResultModel> data, PromotionPrizeMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(DefaultTemplateSheetName);
            var productOrderList = ProductOrderMap.Keys.ToList();

            if (data.Count == 0)
            {
                SetTitle(ViewResources.PromotionPrize, metaModel);
                CreateEmptySheet(workBook, templateSheet, DefaultSheetName);
            }

            data = data.OrderBy(_ => productOrderList.IndexOf(_.Key)).ToDictionary(_ => _.Key, _ => _.Value);

            foreach (var promotionData in data)
            {
                var productName = ViewResources.ResourceManager.GetString(promotionData.Key);
                var title = promotionData.Key == Product.SPR
                    ? ViewResources.SabaPromotionPrizes : productName + ProductOrderMap[promotionData.Key].Item1;

                templateSheet = workBook.GetSheet(ProductOrderMap[promotionData.Key].Item2);

                SetTitle(title, metaModel);

                var sheet = promotionData.Value.Data.Count > 0
                    ? CreateDataSheet(
                        workBook,
                        templateSheet,
                        productName,
                        promotionData.Value,
                        metaModel.DefaultBaseCurrency,
                        promotionData.Key)
                    : CreateEmptySheet(workBook, templateSheet, productName);

                if (promotionData.Key == Product.PPC)
                {
                    _builder.HideColumn(sheet, ColumnPromotionType);
                }
            }

            _builder.RemoveSheet(workBook, DefaultTemplateSheetName);
            _builder.RemoveSheet(workBook, PPCTemplateSheetName);
            return workBook;
        }

        private ISheet CreateDataSheet(
            IWorkbook workbook,
            ISheet templateSheet,
            string sheetName,
            PromotionPrizeResultModel data,
            string currencyName,
            string productKey)
        {
            var showSummaryTotal = productKey != Product.PPC;
            var sheet = _builder.CloneSheet(workbook, templateSheet.SheetName, sheetName);

            AssignHeader(sheet, templateSheet, productKey);
            AssignTitle(sheet, templateSheet);
            AssignDataRow(sheet, templateSheet, data, currencyName, showSummaryTotal, productKey);

            return sheet;
        }

        private ISheet CreateEmptySheet(IWorkbook workbook, ISheet templateSheet, string sheetName)
        {
            var sheet = _builder.CloneSheet(workbook, templateSheet.SheetName, sheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);
            AssignNoInfoRow(sheet, templateSheet);

            return sheet;
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, sourceRowIndex: RowTitleTemplate, destRowIndex: RowTitleTemplate, columnCount: ColumnTemplateCount, value: _title);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        private void SetTitle(string title, PromotionPrizeMetaModel metaModel)
        {
            var currencyTitle = metaModel.CurrencyId <= 0 ? ViewResources.AllCurrencies : $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            _title = $"{title}, {metaModel.FromDate.ToString(FormatDate)} --> {metaModel.ToDate.ToString(FormatDate)} - {currencyTitle}";
        }

        private void AssignHeader(ISheet sheet, ISheet templateSheet, string productKey = "")
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);
            row.Cells[ColumnIndex].SetCellValue("#");
            row.Cells[ColumnDate].SetCellValue(ViewResources.Date);
            row.Cells[ColumnCurrency].SetCellValue(ViewResources.Currency);
            row.Cells[ColumnPromotionType].SetCellValue(ViewResources.PromotionType);
            row.Cells[ColumnExternalId].SetCellValue(ViewResources.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(ViewResources.SystemId);
            if (productKey == Product.PPC)
            {
                row.Cells[ColumnTurnover].SetCellValue(ViewResources.Turnover);
                row.Cells[ColumnContributionAmount].SetCellValue(ViewResources.ContributionAmount);
            }
            else
            {
                row.Cells[ColumnAmount].SetCellValue(ViewResources.Amount);
            }
        }

        private void AssignDataRow(
            ISheet sheet,
            ISheet templateSheet,
            PromotionPrizeResultModel data,
            string currencyName,
            bool showSummaryTotal,
            string productKey = "")
        {
            int index = 0;
            var rowCount = data.Data.Count;

            for (index = 0; index < rowCount; index++)
            {
                var item = data.Data[index];

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(index + 1);
                row.Cells[ColumnDate].SetCellValue(item.Date.ToString(FormatDate));
                row.Cells[ColumnPromotionType].SetCellValue(item.PromotionType);
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
                row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
                if (productKey == Product.PPC)
                {
                    row.Cells[ColumnTurnover].SetCellValue((double)item.Turnover);
                    row.Cells[ColumnContributionAmount].SetCellValue((double)item.ContributionAmount);
                }
                else
                {
                    row.Cells[ColumnAmount].SetCellValue((double)item.Amount);
                }
            }

            if (showSummaryTotal)
            {
                AssignSummaryTotal(sheet, templateSheet, data.SummaryTotal, currencyName, rowCount + RowContentTemplate);
                AssignGrandTotal(sheet, templateSheet, data, currencyName, rowCount + data.SummaryTotal.Count + RowContentTemplate);
                return;
            }

            AssignGrandTotal(sheet, templateSheet, data, currencyName, rowCount + RowContentTemplate, productKey);
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        private void AssignSummaryTotal(ISheet sheet, ISheet templateSheet, IList<PromotionPrize> summaryTotal, string currencyName, int rowIndex)
        {
            var summaryRowIndex = rowIndex;
            foreach (var summary in summaryTotal)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templateSheet, RowTotal, summaryRowIndex);
                row.Cells[ColumnAmount].SetCellValue((double)summary.Amount);
                row.Cells[ColumnPromotionType].SetCellValue(summary.PromotionType);

                // Merge column
                _builder.MergeCells(sheet, firstRow: summaryRowIndex, lastRow: summaryRowIndex, firstCol: ColumnPromotionType, lastCol: ColumnSystemId);

                summaryRowIndex++;
            }

            if (summaryRowIndex > rowIndex)
            {
                var firstSummaryRow = sheet.GetRow(rowIndex);
                firstSummaryRow.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, currencyName));
                _builder.MergeCells(sheet, firstRow: rowIndex, lastRow: summaryRowIndex - 1, firstCol: ColumnIndex, lastCol: ColumnDate);
            }
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, PromotionPrizeResultModel data, string currency, int rowIndex, string productKey = "")
        {
            // Copy template
            var row = _builder.CopyRow(sheet, templateSheet, RowGrandTotal, rowIndex);
            if (productKey == Product.PPC)
            {
                row.Cells[ColumnTurnover].SetCellValue((double)data.TurnoverTotal);
                row.Cells[ColumnContributionAmount].SetCellValue((double)data.ContributionAmountTotal);
            }
            else
            {
                row.Cells[ColumnAmount].SetCellValue((double)data.GrandTotal);
            }
            row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, currency));

            // Merge first two column
            _builder.MergeCells(sheet, firstRow: rowIndex, lastRow: rowIndex, firstCol: ColumnIndex, lastCol: ColumnSystemId);
        }
    }
}
