namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.Linq;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Helpers;

    using NPOI.SS.UserModel;

    public class PlatformWinLossSummaryExcel : IPlatformWinLossSummaryExcel
    {
        private const string TemplateSheetName = "Template";
        private const int ColumnPlatform = 0;
        private const int ColumnTurnover = 1;
        private const int ColumnTurnoverPercent = 2;
        private const int ColumnTotalTransaction = 3;
        private const int ColumnCustomerWinLoss = 4;
        private const int ColumnCustomerComm = 5;
        private const int ColumnCustomerTotal = 6;
        private const int ColumnLicenseeWinLoss = 7;

        private const int ColumnNoContent = 8;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowNoInfoTemplate = 6;
        private const int RowTotal = 5;

        private const int RowParentContentTemplate = 3;
        private const int RowChildContentTemplate = 4;

        private readonly IExcelBuilder _builder;

        public PlatformWinLossSummaryExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public IWorkbook Export(string template, ResultModel<PlatformWinLossSummary> data, PlatformWinLossMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, "Data");
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

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private static void AssignCellData(IRow row, PlatformWinLossSummary data)
        {
            row.Cells[ColumnPlatform].SetCellValue(data.Name);
            row.Cells[ColumnTurnover].SetCellValue((double)data.Turnover);
            row.Cells[ColumnTurnoverPercent].SetCellValue(string.Format("{0:0.00}%", data.TurnoverPercent));
            row.Cells[ColumnTotalTransaction].SetCellValue(data.TotalTransactions);
            row.Cells[ColumnCustomerWinLoss].SetCellValue((double)data.CustomerWinLoss);
            row.Cells[ColumnCustomerComm].SetCellValue((double)data.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)data.CustomerTotal);
            row.Cells[ColumnLicenseeWinLoss].SetCellValue((double)data.LicenseeWL);
        }

        private static string GetProductTitle(IList<ProductOption> productOptions)
        {
            var selectedProducts = productOptions.Where(x => x.Status);
            if (selectedProducts.Count() == productOptions.Count())
            {
                return string.Format("{0}: {1}", ViewResources.Product, ViewResources.All);
            }

            var text = selectedProducts
                .Aggregate(string.Empty, (current, x) => current + (string.IsNullOrEmpty(current) ? x.Code : ", " + x.Code));

            return string.Format("{0}: {1}", ViewResources.Product, text);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, PlatformWinLossMetaModel metaModel)
        {
            var platformText = ViewResources.Platform + ": " + DataHelper.GetPlatformName(metaModel.PlatformType);
            var currencyText = metaModel.CurrencyId == 0 ?
                ViewResources.AllCurrencies :
                string.Format("{0} {1}", ViewResources.Currency, metaModel.CurrencyName);

            var baseCurrencyText = string.Format(
                ViewResources.Viewin,
                !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName);

            var reportTitle = string.Format(
                                        ViewResources.PlatformWinLossTitle,
                                        metaModel.FromDate.ToString("MM/dd/yyyy"),
                                        metaModel.ToDate.ToString("MM/dd/yyyy"),
                                        currencyText,
                                        baseCurrencyText,
                                        platformText) + ", " + GetProductTitle(metaModel.ProductOptions);

            _builder.SetRowValue(sheet, templateSheet, RowTitleTemplate, RowTitleTemplate, ColumnNoContent, reportTitle);

            // Freeze 2 rows title and header
            sheet.CreateFreezePane(0, RowParentContentTemplate);

            _builder.ClearTemplate(sheet, RowParentContentTemplate);
        }

        private void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var firstRow = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);
            firstRow.Cells[ColumnPlatform].SetCellValue(ViewResources.Platform);
            firstRow.Cells[ColumnTurnover].SetCellValue(ViewResources.Turnover);
            firstRow.Cells[ColumnTurnoverPercent].SetCellValue(ViewResources.TurnoverPercent);
            firstRow.Cells[ColumnTotalTransaction].SetCellValue(ViewResources.TotalTransaction);
            firstRow.Cells[ColumnCustomerWinLoss].SetCellValue(ViewResources.CustomerW_L);
            firstRow.Cells[ColumnCustomerComm].SetCellValue(ViewResources.CustomerW_L);
            firstRow.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.CustomerW_L);
            firstRow.Cells[ColumnLicenseeWinLoss].SetCellValue(ViewResources.LicenseeW_L);

            var secondRow = _builder.CopyRow(templateSheet, templateSheet, sourceRowIndex: RowHeaderTemplate + 1, destRowIndex: RowHeaderTemplate + 1);
            secondRow.Cells[ColumnCustomerWinLoss].SetCellValue(ViewResources.WinLoss);
            secondRow.Cells[ColumnCustomerComm].SetCellValue(ViewResources.Comm);
            secondRow.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, ResultModel<PlatformWinLossSummary> data)
        {
            var rowIndex = RowParentContentTemplate;
            foreach (var item in data.DataList)
            {
                var row = item.Level == 0 ?
                    _builder.CopyRow(sheet, templateSheet, RowParentContentTemplate, rowIndex) :
                    _builder.CopyRow(sheet, templateSheet, RowChildContentTemplate, rowIndex);

                AssignCellData(row, item);
                rowIndex++;
            }

            var totalRow = _builder.CopyRow(sheet, templateSheet, RowTotal, rowIndex);
            data.TotalData.Name = ViewResources.Total;
            AssignCellData(totalRow, data.TotalData);
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowParentContentTemplate, ColumnNoContent, ViewResources.NoInfo);
        }
    }
}