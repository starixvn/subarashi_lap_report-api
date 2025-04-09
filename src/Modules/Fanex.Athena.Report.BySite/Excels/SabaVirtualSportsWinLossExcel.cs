namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Excel.Interfaces;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Options;
    using Resources;
    using Resources.Constants;

    public class SabaVirtualSportsWinLossExcel : ISabaVirtualSportsWinLossExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        public const string FormatDate = "MM/dd/yyyy";

        public const int PageSize = 30000;

        private const int ColumnIndex = 0;

        private const int ColumnDate = 1;

        private const int ColumnExternalId = 2;

        private const int ColumnSystemId = 3;

        private const int ColumnCurrency = 4;

        private const int ColumnTurnover = 5;

        private const int ColumnTotalTransaction = 6;

        private const int ColumnCustomerWl = 7;

        private const int ColumnCustomerComm = 8;

        private const int ColumnCustomerTotal = 9;

        private const int ColumnLicenseeWlSportsBook = 10;

        private const int ColumnLicenseeWlCashout = 11;

        private const int ColumnLicenseeWlTotal = 12;

        private const int ColumnCashOutCommission = 13;

        private const int ColumnTemplateCount = 14;

        public const int RowTitleTemplate = 0;

        private const int RowHeaderTemplate = 1;

        private const int RowContentTemplate = 3;

        private const int RowGrantTotal = 6;

        private const int RowNoInfoTemplate = 7;

        private const int RowTotalNoSolidBorder = 5;

        private const int RowTotalSolidBorder = 4;

        private readonly IExcelBuilder _builder;

        private SortableList<SabaVirtualSportsWinLossDetail> _data;

        private SabaVirtualSportsWinLossMetaModel _metaModel;

        private SabaVirtualSportsWinLossExcelOption _options;

        private IDictionary<string, SabaVirtualSportsWinLossDetail> _summaryData;

        private IEnumerable<SabaVirtualSportsWinLossDetail> _totalData;

        public SabaVirtualSportsWinLossExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(
            string template,
            SortableReportResultModel<SabaVirtualSportsWinLossDetail> data,
            SabaVirtualSportsWinLossMetaModel metaModel,
            SabaVirtualSportsWinLossExcelOption option)
        {
            _data = data.DataList;
            _totalData = data.TotalDataList;
            _summaryData = data.SummaryTotal;
            _options = option;
            _metaModel = metaModel;

            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        public IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);

            if (_data.Any())
            {
                AssignSheetData(sheet, templateSheet, _data);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            ProcessUi(sheet);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected internal virtual int AssignGrandTotalInEurData(ISheet sheet, ISheet templatSheet, int rowIndex)
        {
            var currencies = _summaryData.Keys.Reverse();

            foreach (var currency in currencies)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templatSheet, RowGrantTotal, rowIndex);
                AssignCommonData(row, _summaryData[currency]);
                row.Cells[ColumnIndex].SetCellValue(string.Format(ViewResources.Totalin, currency.ToUpperInvariant()));

                // Merge first two column
                _builder.MergeCols(sheet, ColumnIndex, ColumnCurrency, rowIndex);
                rowIndex++;
            }

            return rowIndex;
        }

        protected internal virtual int AssignTotalData(ISheet sheet, ISheet templateSheet, int rowIndex)
        {
            var beginTotalRow = rowIndex;

            foreach (var totalCurrency in _totalData)
            {
                IRow row;

                if (rowIndex == beginTotalRow)
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalSolidBorder, rowIndex);
                    row.Cells[0].SetCellValue(ViewResources.Total);
                }
                else
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalNoSolidBorder, rowIndex);
                }

                row.Cells[ColumnCurrency].SetCellValue(totalCurrency.CurrencyName);

                AssignCommonData(row, totalCurrency);

                rowIndex++;
            }

            // Merge first two column
            const int LastTotalMergedCol = 3;
            _builder.MergeCells(sheet, rowIndex - _totalData.Count(), rowIndex - 1, 0, LastTotalMergedCol);

            return rowIndex;
        }

        protected internal virtual void HideColumnsByLevel(ISheet sheet)
        {
            switch (_metaModel.DrillDownLevel)
            {
                case 0:
                    _builder.HideColumn(sheet, ColumnExternalId, ColumnSystemId);
                    break;

                case 1:
                    _builder.HideColumn(sheet, ColumnExternalId, ColumnDate);
                    break;

                default:
                    _builder.HideColumn(sheet, ColumnDate);
                    break;
            }
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            // row 1
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);
            row.Cells[ColumnIndex].SetCellValue("#");
            row.Cells[ColumnDate].SetCellValue(ViewResources.Date);
            row.Cells[ColumnExternalId].SetCellValue(ViewResources.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(ViewResources.SystemId);
            row.Cells[ColumnCurrency].SetCellValue(ViewResources.Currency);
            row.Cells[ColumnTurnover].SetCellValue(ViewResources.Turnover);
            row.Cells[ColumnTotalTransaction].SetCellValue(ViewResources.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.CustomerW_L);
            row.Cells[ColumnLicenseeWlSportsBook].SetCellValue(ViewResources.LicenseeW_L);
            row.Cells[ColumnLicenseeWlTotal].SetCellValue(ViewResources.LicenseeW_L);
            row.Cells[ColumnCashOutCommission].SetCellValue(ViewResources.CashOutCommission);

            var row2 = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate + 1, RowHeaderTemplate + 1);

            // row 2 - CustomerWL
            row2.Cells[ColumnCustomerWl].SetCellValue(ViewResources.WinLoss);
            row2.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            row2.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);

            // row 2 - LicenseeWL
            row2.Cells[ColumnLicenseeWlSportsBook].SetCellValue(ViewResources.ProductSBVS);
            row2.Cells[ColumnLicenseeWlCashout].SetCellValue(ViewResources.ProductCO);
            row2.Cells[ColumnLicenseeWlTotal].SetCellValue(ViewResources.Total);
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var currency = _metaModel.CurrencyId switch
            {
                0 => ViewResources.AllCurrencies,
                > 0 => _metaModel.CurrencyName,
                _ => _metaModel.BaseCurrency
            };

            var reportTitle = string.Format(
                "{0}, {1:MM/dd/yyyy} → {2:MM/dd/yyyy}, {3}: {4}{5}",
                ViewResources.SabaVirtualSportsWinLoss,
                _metaModel.FromDate,
                _metaModel.ToDate,
                ViewResources.Currency,
                currency,
                string.IsNullOrWhiteSpace(_metaModel.UserName) ? string.Empty : $", {ViewResources.Username}: {_metaModel.UserName}");

            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        protected void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableList<SabaVirtualSportsWinLossDetail> data)
        {
            var rowSpan = 1;
            var rowIndex = 1;
            var indexDetail = 1;
            int index;
            var rowCount = data.Count;

            for (index = 0; index < rowCount; index++)
            {
                var item = data.ElementAt(index);

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);

                var reportDateString = _metaModel.ReportBy == ReportByType.Monthly ? item.Date.ToString("MM/yyyy") : item.Date.ToShortDateString();

                // Merge first two columns
                var needMerge = index >= rowCount - 1 || item.Date.Date != data.ElementAt(index + 1).Date.Date;

                // Merge cell
                if (needMerge && rowSpan > 1)
                {
                    MergeCells(sheet, rowSpan, index);
                }

                rowSpan = needMerge ? 1 : rowSpan + 1;

                var indexid = _metaModel.DrillDownLevel == 0 ? rowIndex : indexDetail++;
                row.Cells[ColumnIndex].SetCellValue(indexid);
                row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
                row.Cells[ColumnDate].SetCellValue(reportDateString);
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);

                AssignCommonData(row, item);

                rowIndex = needMerge ? rowIndex + 1 : rowIndex;
            }

            rowIndex = AssignTotalData(sheet, templateSheet, index + RowContentTemplate);
            AssignGrandTotalInEurData(sheet, templateSheet, rowIndex);
        }

        protected void AssignCommonData(IRow row, SabaVirtualSportsWinLossDetail data)
        {
            row.Cells[ColumnTurnover].SetCellValue((double)data.TurnOver.Sum(a => a.Value));
            row.Cells[ColumnTotalTransaction].SetCellValue((double)data.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue((double)data.CustomerWinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue((double)data.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)data.CustomerTotal);
            row.Cells[ColumnLicenseeWlSportsBook].SetCellValue((double)data.LicenseeWL[Product.SB]);
            row.Cells[ColumnLicenseeWlCashout].SetCellValue((double)data.LicenseeWL[Product.CO]);
            row.Cells[ColumnLicenseeWlTotal].SetCellValue((double)data.LicenseeWL.Sum(a => a.Value));
            row.Cells[ColumnCashOutCommission].SetCellValue((double)data.CashOutCommission);
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet tempalteSheet)
        {
            _builder.SetRowValue(sheet, tempalteSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        protected void ProcessUi(ISheet sheet)
        {
            if (!_options.CustomerCommVisible)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerComm);
                _builder.MergeRows(sheet, RowHeaderTemplate, RowHeaderTemplate + 1, ColumnCustomerWl);
            }

            if (!_options.CoVisible)
            {
                _builder.HideColumn(sheet, ColumnCashOutCommission);
            }

            HideColumnsByLevel(sheet);
        }

        private void MergeCells(ISheet sheet, int rowSpan, int index)
        {
            var mergedRowIndex = _metaModel.DrillDownLevel == (int)ReportLevel.Main ? ColumnIndex : ColumnDate;

            _builder.MergeRows(sheet, index + RowContentTemplate - rowSpan + 1, index + RowContentTemplate, mergedRowIndex);
        }
    }
}