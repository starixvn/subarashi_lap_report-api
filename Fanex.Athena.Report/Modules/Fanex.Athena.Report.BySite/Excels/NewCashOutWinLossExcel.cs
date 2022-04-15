namespace Fanex.Athena.Report.BySite.Excels
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using NPOI.SS.UserModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class NewCashOutWinLossExcel : INewCashOutWinLossExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnTurnOver = 5;
        private const int ColumnTotalTransaction = 6;
        private const int ColumnCustomerWinLoss = 7;
        private const int ColumnCustomerComm = 8;
        private const int ColumnCustomerTotal = 9;
        private const int ColumnLicenseeWL = 10;
        private const int ColumnBaComm = 11;
        private const int ColumnAmtDueToBA = 12;

        private const int RowNoInfoTemplate = 7;
        private const int ColumnTemplateCount = 13;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalCurrencyTemplate = 4;
        private const int RowTotalCurrencyNoSolidBorder = 5;
        private const int RowGrantTotalTemplate = 6;

        private readonly IExcelBuilder _builder;

        public NewCashOutWinLossExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<CashOutWinLoss> data, NewCashOutWinLossMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<CashOutWinLoss> data, NewCashOutWinLossMetaModel metaModel)
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
            row.Cells[ColumnIndex].SetCellValue("#");
            row.Cells[ColumnDate].SetCellValue(ViewResources.Date);
            row.Cells[ColumnExternalId].SetCellValue(ViewResources.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(ViewResources.SystemId);
            row.Cells[ColumnCurrency].SetCellValue(ViewResources.Currency);
            row.Cells[ColumnTurnOver].SetCellValue(ViewResources.Turnover);
            row.Cells[ColumnTotalTransaction].SetCellValue(ViewResources.TotalTransaction);
            row.Cells[ColumnCustomerWinLoss].SetCellValue(ViewResources.CustomerW_L);
            row.Cells[ColumnLicenseeWL].SetCellValue(ViewResources.LicenseeW_L);
            row.Cells[ColumnBaComm].SetCellValue(ViewResources.BACommission);
            row.Cells[ColumnAmtDueToBA].SetCellValue(ViewResources.AmtDueToBA);

            // row 2
            var row2 = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate + 1, destRowIndex: RowHeaderTemplate + 1);
            row2.Cells[ColumnCustomerWinLoss].SetCellValue(ViewResources.WinLoss);
            row2.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row2.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<CashOutWinLoss> data)
        {
            int index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(++index);
                AssignCellData(row, item);
            }

            AssignTotalCurrency(sheet, templateSheet, data, index + RowContentTemplate);
            AssignGrandTotal(sheet, templateSheet, data, index + RowContentTemplate + data.TotalDataList.Count());
        }

        private static void AssignCellData(IRow row, CashOutWinLoss item)
        {
            row.Cells[ColumnDate].SetCellValue(item.DateString);
            row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
            row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
            row.Cells[ColumnTurnOver].SetCellValue((double)item.Turnover);
            row.Cells[ColumnTotalTransaction].SetCellValue(item.TotalTransaction);
            row.Cells[ColumnCustomerWinLoss].SetCellValue((double)item.CustomerWL);
            row.Cells[ColumnCustomerComm].SetCellValue((double)item.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)item.CustomerTotal);
            row.Cells[ColumnLicenseeWL].SetCellValue((double)item.LicenseeWL);
            row.Cells[ColumnBaComm].SetCellValue((double)item.BAComm);
            row.Cells[ColumnAmtDueToBA].SetCellValue((double)item.AmtDueToBA);
        }

        private void AssignTotalCurrency(ISheet sheet, ISheet templateSheet, SortableReportResultModel<CashOutWinLoss> data, int index)
        {
            int beginTotalRow = index;

            foreach (var totalCurrency in data.TotalDataList)
            {
                IRow row;

                if (index == beginTotalRow)
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalCurrencyTemplate, index);
                    row.Cells[0].SetCellValue(ViewResources.Total);
                }
                else
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalCurrencyNoSolidBorder, index);
                }

                AssignCellData(row, totalCurrency);
                row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);
                row.Cells[ColumnCurrency].SetCellValue(totalCurrency.CurrencyName);
                _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnSystemId);
                index++;
            }
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<CashOutWinLoss> data, int index)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

            AssignCellData(row, data.TotalData);

            row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, data.TotalData.CurrencyName));

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnCurrency);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, NewCashOutWinLossMetaModel metaModel)
        {
            var displayedCurrency = string.IsNullOrEmpty(metaModel.BaseCurrency) ? metaModel.CurrencyName : metaModel.BaseCurrency.ToUpperInvariant();
            var currencyText = string.Format(ViewResources.Viewin, displayedCurrency);

            var fromDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownFromDate : metaModel.FromDate;
            var toDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownToDate : metaModel.ToDate;

            var reportTitle = $"{ViewResources.CashOutWinLoss}, {fromDate:MM/dd/yyyy} -> {toDate:MM/dd/yyyy}, {currencyText}";

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

        private void HideColumns(ISheet sheet, NewCashOutWinLossMetaModel metaModel)
        {
            if (metaModel.DrillDownLevel == (int)ReportLevel.Main)
            {
                _builder.HideColumn(sheet, ColumnExternalId, ColumnSystemId);
            }
            else
            {
                _builder.HideColumn(sheet, ColumnDate);
            }
        }
    }
}