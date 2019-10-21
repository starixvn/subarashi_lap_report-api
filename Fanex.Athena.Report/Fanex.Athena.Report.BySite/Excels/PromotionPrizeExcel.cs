namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Resources;

    using NPOI.SS.UserModel;

    public class PromotionPrizeExcel : IPromotionPrizeExcel
    {
        private const string TemplateSheetName = "Template";
        private const string DefaultSheetName = "Data";
        public const string FormatDate = "MM/dd/yyyy";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnCurrency = 2;
        private const int ColumnExternalId = 3;
        private const int ColumnSystemId = 4;
        private const int ColumnAmount = 5;

        private const int ColumnTemplateCount = 6;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 2;
        private const int RowTotal = 4;
        private const int RowNoInfoTemplate = 7;

        private readonly IExcelBuilder _builder;

        private string _title;

        public PromotionPrizeExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public IWorkbook Export(string template, IDictionary<string, PromotionPrizeResultModel> data, PromotionPrizeMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            if (data.Count == 0)
            {
                SetTitle(ViewResources.PromotionPrize, metaModel);
                CreateEmptySheet(workBook, templateSheet, DefaultSheetName);
            }

            foreach (var promotionData in data)
            {
                var productName = ViewResources.ResourceManager.GetString(promotionData.Key);
                var title = productName + " - " + @ViewResources.PromotionPrize;
                SetTitle(title, metaModel);

                if (promotionData.Value.Data.Count > 0)
                {
                    CreateDataSheet(workBook, templateSheet, productName, promotionData.Value);
                }
                else
                {
                    CreateEmptySheet(workBook, templateSheet, productName);
                }
            }

            _builder.RemoveSheet(workBook, TemplateSheetName);
            return workBook;
        }

        private void CreateDataSheet(IWorkbook workbook, ISheet templateSheet, string sheetName, PromotionPrizeResultModel data)
        {
            var sheet = _builder.CloneSheet(workbook, templateSheet.SheetName, sheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);
            AssignDataRow(sheet, templateSheet, data);
        }

        private void CreateEmptySheet(IWorkbook workbook, ISheet templateSheet, string sheetName)
        {
            var sheet = _builder.CloneSheet(workbook, templateSheet.SheetName, sheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);
            AssignNoInfoRow(sheet, templateSheet);
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

        private void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);
            row.Cells[ColumnIndex].SetCellValue("#");
            row.Cells[ColumnDate].SetCellValue(ViewResources.Date);
            row.Cells[ColumnCurrency].SetCellValue(ViewResources.Currency);
            row.Cells[ColumnExternalId].SetCellValue(ViewResources.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(ViewResources.SystemId);
            row.Cells[ColumnAmount].SetCellValue(ViewResources.Amount);
        }

        private void AssignDataRow(ISheet sheet, ISheet templateSheet, PromotionPrizeResultModel data)
        {
            int index = 0;
            var rowCount = data.Data.Count;

            for (index = 0; index < rowCount; index++)
            {
                var item = data.Data[index];

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(index + 1);
                row.Cells[ColumnDate].SetCellValue(item.Date.ToString(FormatDate));
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
                row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
                row.Cells[ColumnAmount].SetCellValue((double)item.Amount);
            }

            AssignGrandTotal(sheet, templateSheet, data.SummaryTotal, index + RowContentTemplate);
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, IDictionary<string, PromotionPrize> summaryTotal, int rowIndex)
        {
            foreach (var summary in summaryTotal)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templateSheet, RowTotal, rowIndex);
                row.Cells[ColumnAmount].SetCellValue((double)summary.Value.Amount);
                row.Cells[ColumnIndex].SetCellValue(ViewResources.Totalin + " " + summary.Key);

                // Merge first two column
                _builder.MergeCells(sheet, firstRow: rowIndex, lastRow: rowIndex, firstCol: ColumnIndex, lastCol: ColumnSystemId);
                rowIndex++;
            }
        }
    }
}