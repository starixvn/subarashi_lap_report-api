namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Excel.Interfaces;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Resources;
    using Resources.Constants;

    public class ExtraPayoutDetailExcel : IExtraPayoutDetailExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnRefNo = 2;
        private const int ColumnBetID = 3;
        private const int ColumnProduct = 4;
        private const int ColumnPromotionType = 5;
        private const int ColumnTurnOver = 6;
        private const int ColumnCustomerAmount = 7;
        private const int ColumnLicSharingAmount = 8;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 9;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowGrantTotalTemplate = 5;

        private readonly IExcelBuilder _builder;

        public ExtraPayoutDetailExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<ExtraPayoutDetail> data, ExtraPayoutDetailMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<ExtraPayoutDetail> data, ExtraPayoutDetailMetaModel metaModel)
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
                ViewResources.RefNo,
                ViewResources.BetID,
                ViewResources.Product,
                ViewResources.PromotionType,
                ViewResources.Turnover,
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<ExtraPayoutDetail> data, ExtraPayoutDetailMetaModel metaModel)
        {
            int index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(++index);
                AssignCellData(row, item, true);
            }
            AssignGrandTotal(sheet, templateSheet, data, index + RowContentTemplate, metaModel);
        }

        private static void AssignCellData(IRow row, ExtraPayoutDetail item, bool hideLicSharingAmountForTip = false)
        {
            row.Cells[ColumnDate].SetCellValue(item.DateString);
            row.Cells[ColumnRefNo].SetCellValue(item.Refno);
            row.Cells[ColumnBetID].SetCellValue(item.BetID);
            row.Cells[ColumnProduct].SetCellValue(item.ProductName);
            row.Cells[ColumnPromotionType].SetCellValue(item.PromotionName);
            row.Cells[ColumnTurnOver].SetCellValue((double)item.Turnover);
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

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<ExtraPayoutDetail> data, int index, ExtraPayoutDetailMetaModel metaModel)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

            AssignCellData(row, data.TotalData);

            row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, metaModel.BaseCurrency.ToUpperInvariant()));

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnPromotionType);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, ExtraPayoutDetailMetaModel metaModel)
        {
            var currencyText = string.Format(ViewResources.Viewin, metaModel.CurrencyName.ToUpperInvariant());
            var usernameText = $"- {metaModel.ExternalId} ({metaModel.SystemId})";

            var fromDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownFromDate : metaModel.FromDate;
            var toDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownToDate : metaModel.ToDate;

            var reportTitle = $"{ViewResources.PromotionDetails}, {fromDate.ToString("MM/dd/yyyy")} -> {toDate.ToString("MM/dd/yyyy")} {usernameText} - {currencyText}";

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