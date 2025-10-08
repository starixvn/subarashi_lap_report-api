namespace Fanex.Athena.Report.BySite.Excels
{
    using Common.Excel.Interfaces;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Resources;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class OddsBoostBonusDetailExcel : IOddsBoostBonusDetailExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnRefNo = 4;
        private const int ColumnProduct = 5;
        private const int ColumnPromotionType = 6;
        private const int ColumnCustomerAmount = 7;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 8;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowGrantTotalTemplate = 5;

        private readonly IExcelBuilder _builder;

        public OddsBoostBonusDetailExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<OddsBoostBonusDetail> data, ExtraPayoutMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<OddsBoostBonusDetail> data, ExtraPayoutMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet, metaModel);

            if (data.DataList.Any())
            {
                AssignSheetData(sheet, templateSheet, data, metaModel);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

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
                ViewResources.RefNo,
                ViewResources.Product,
                ViewResources.PromotionType,
                ViewResources.CustomerAmount
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<OddsBoostBonusDetail> data, ExtraPayoutMetaModel metaModel)
        {
            int index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(++index);
                AssignCellData(row, item);
            }
            AssignGrandTotal(sheet, templateSheet, data, index + RowContentTemplate, metaModel);
        }

        private static void AssignCellData(IRow row, OddsBoostBonusDetail item)
        {
            row.Cells[ColumnDate].SetCellValue(item.DateString);
            row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
            row.Cells[ColumnRefNo].SetCellValue(item.RefNo);
            row.Cells[ColumnProduct].SetCellValue(item.ProductName);
            row.Cells[ColumnPromotionType].SetCellValue(item.PromotionType);
            row.Cells[ColumnCustomerAmount].SetCellValue((double)item.CustomerAmount);
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<OddsBoostBonusDetail> data, int index, ExtraPayoutMetaModel metaModel)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

            AssignCellData(row, data.TotalData);

            row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, metaModel.BaseCurrency.ToUpperInvariant()));

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnPromotionType);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, ExtraPayoutMetaModel metaModel)
        {
            var currencyText = string.Format(ViewResources.Viewin, metaModel.CurrencyName.ToUpperInvariant());
            var dateText = $"{metaModel.DrillDownFromDate.ToString("MM/dd/yyyy")} -> {metaModel.DrillDownToDate.ToString("MM/dd/yyyy")}";

            var reportTitle = $"{ViewResources.PromotionDetails}, {dateText} - {ViewResources.OddsBoostBonus} - {currencyText}";

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