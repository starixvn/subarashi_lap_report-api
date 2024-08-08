namespace Fanex.Athena.Report.BySite.Excels
{
    using System.IO;
    using System.Linq;
    using Common.Excel;
    using Common.Excel.Interfaces;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using NPOI.SS.UserModel;
    using Options;
    using Resources;

    public class ThirdPartyWinLossExcel<T> : IThirdPartyWinLossExcel<T> where T : ThirdPartyWinLoss
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        private const int ColumnTemplateCount = 12;

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnTurnover = 5;
        private const int ColumnTotalTransaction = 6;
        private const int ColumnCustomerWl = 7;
        private const int ColumnLicenseeWl = 8;
        private const int ColumnContributionWl = 9;
        private const int ColumnPromotionWl = 10;
        private const int ColumnJackpotPrizeWl = 11;
        private const int ColumnTotalWl = 12;

        public const int RowTitleTemplate = 0;
        public const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 3;
        private const int RowTotalSolidBorder = 4;
        private const int RowTotalNoSolidBorder = 5;
        private const int RowGrantTotal = 6;
        private const int RowNoInfoTemplate = 7;

        private readonly IExcelBuilder _builder;

        private ThirdPartyWinLossMetaModel _metaModel;

        private ThirdPartyWinLossExcelOption _option;

        private ReportResultModel<T> _resultData;

        public ThirdPartyWinLossExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, ReportResultModel<T> data, ThirdPartyWinLossMetaModel metaModel, ThirdPartyWinLossExcelOption option)
        {
            _resultData = data;
            _metaModel = metaModel;
            _option = option;

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

            if (_resultData.DataList.Any())
            {
                AssignSheetData(sheet, templateSheet);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            ProcessUi(sheet);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected internal virtual void AssignGrandTotalInEurData(ISheet sheet, ISheet templateSheet, int rowIndex)
        {
            var currencyList = _resultData.SummaryTotal.Keys.ToList();
            currencyList.Reverse();

            foreach (var currency in currencyList)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotal, rowIndex);
                row.Cells[ColumnIndex].SetCellValue(ViewResources.Totalin + " " + currency.ToUpperInvariant());
                AssignCommonData(row, _resultData.SummaryTotal[currency]);

                // Merge first two column
                _builder.MergeCols(sheet, ColumnIndex, ColumnCurrency, rowIndex);
                rowIndex++;
            }
        }

        protected internal virtual int AssignTotalData(ISheet sheet, ISheet templateSheet, int rowIndex)
        {
            var beginTotalRow = rowIndex;
            var totalCurrencyList = _resultData.TotalDataList;

            foreach (var totalCurrency in totalCurrencyList)
            {
                // Copy template
                var row = _builder.CopyRow(sheet, templateSheet, RowTotalNoSolidBorder, rowIndex);

                if (rowIndex == beginTotalRow)
                {
                    row = _builder.CopyRow(sheet, templateSheet, RowTotalSolidBorder, rowIndex);
                    row.Cells[0].SetCellValue(ViewResources.Total);
                }

                row.Cells[ColumnCurrency].SetCellValue(totalCurrency.CurrencyName);

                AssignCommonData(row, totalCurrency);

                rowIndex++;
            }

            // Merge first two column
            const int LastIndexMergedCol = 3;
            _builder.MergeCells(sheet, rowIndex - totalCurrencyList.Count(), rowIndex - 1, 0, LastIndexMergedCol);

            return rowIndex;
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);
            row.Cells[ColumnIndex].SetCellValue("#");
            row.Cells[ColumnExternalId].SetCellValue(ViewResources.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(ViewResources.SystemId);
            row.Cells[ColumnDate].SetCellValue(ViewResources.Date);
            row.Cells[ColumnCurrency].SetCellValue(ViewResources.Currency);
            row.Cells[ColumnTurnover].SetCellValue(ViewResources.Turnover);
            row.Cells[ColumnTotalTransaction].SetCellValue(ViewResources.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue(ViewResources.PlayerWL);
            row.Cells[ColumnLicenseeWl].SetCellValue(ViewResources.Licensee);

            var row2 = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate + 1, RowHeaderTemplate + 1);
            row2.Cells[ColumnLicenseeWl].SetCellValue(ViewResources.W_L);
            row2.Cells[ColumnContributionWl].SetCellValue(ViewResources.Contribution);
            row2.Cells[ColumnPromotionWl].SetCellValue(ViewResources.Promotion);
            row2.Cells[ColumnJackpotPrizeWl].SetCellValue(ViewResources.JackpotPrize);
            row2.Cells[ColumnTotalWl].SetCellValue(ViewResources.Total);

            PostAssignHeader(row);
        }

        protected virtual void PostAssignHeader(IRow row)
        {
            // implement in children
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
            ProcessUi(sheet);
        }

        protected void AssignSheetData(ISheet sheet, ISheet templateSheet)
        {
            var rowIndex = 1;
            int index;
            var dataList = _resultData.DataList;
            var rowCount = dataList.Count();

            for (index = 0; index < rowCount; index++)
            {
                var item = dataList.ElementAt(index);

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);

                var dateStringFormat = _metaModel.ReportBy == 1 ? "MM/dd/yyyy" : "MM/yyyy";
                var reportDateString = item.Date?.ToString(dateStringFormat);

                row.Cells[ColumnIndex].SetCellValue(item.Index);
                row.Cells[ColumnDate].SetCellValue(reportDateString);
                row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
                AssignCommonData(row, item);
                rowIndex++;

                if (item.RowSpan > 1)
                {
                    var currentRow = index + RowContentTemplate;
                    _builder.MergeRows(sheet, currentRow, currentRow + item.RowSpan - 1, ColumnIndex);
                    _builder.MergeRows(sheet, currentRow, currentRow + item.RowSpan - 1, ColumnDate);
                }
            }

            rowIndex = AssignTotalData(sheet, templateSheet, index + RowContentTemplate);
            AssignGrandTotalInEurData(sheet, templateSheet, rowIndex);
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var reportName = _metaModel.ProductName + " " + ViewResources.WinLoss;
            var currencyTitle = ExcelHelper.GetTitleCurrencyName(_option);
            var reportTitle = $"{reportName}, {_metaModel.FromDate:MM/dd/yyyy} --> {_metaModel.ToDate:MM/dd/yyyy}, {currencyTitle}";

            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowContentTemplate);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        protected void ProcessUi(ISheet sheet)
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

        protected virtual void AssignCommonData(IRow row, ThirdPartyWinLoss data)
        {
            row.Cells[ColumnTurnover].SetCellValue((double)data.TurnOver);
            row.Cells[ColumnTotalTransaction].SetCellValue((double)data.TotalTransaction);
            row.Cells[ColumnCustomerWl].SetCellValue((double)data.CustomerWinLoss);
            row.Cells[ColumnLicenseeWl].SetCellValue((double)data.LicenseeWL);
            row.Cells[ColumnContributionWl].SetCellValue((double)data.ContributionWL);
            row.Cells[ColumnPromotionWl].SetCellValue((double)data.PromotionWL);
            row.Cells[ColumnJackpotPrizeWl].SetCellValue((double)data.JackpotWL);
            row.Cells[ColumnTotalWl].SetCellValue((double)data.TotalWL);
        }
    }
}