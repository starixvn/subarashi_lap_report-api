namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Athena.Report.Resources.Helpers;

    using NPOI.SS.UserModel;

    public class PlatformWinLossMemberExcel : IPlatformWinLossMemberExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 1;
        private const int ColumnSystemId = 2;
        private const int ColumnCurrency = 3;
        private const int ColumnTurnover = 4;
        private const int ColumnTotalTrans = 5;
        private const int ColumnCustomerWl = 6;
        private const int ColumnCustomerCommission = 7;
        private const int ColumnCustomerTotal = 8;
        private const int ColumnLicenseeWl = 9;
        private const int ColumnBaCommission = 10;
        private const int ColumnAmtDueToBa = 11;
        private const int ColumnTemplateCount = 12;

        private const int RowNoInfoTemplate = 5;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;

        private readonly IExcelBuilder _builder;

        public PlatformWinLossMemberExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableResultModel<PlatformWinLossMember> data, PlatformWinLossMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableResultModel<PlatformWinLossMember> data, PlatformWinLossMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet, metaModel);

            if (data.DataList.Any())
            {
                SortMemberData(data.DataList as ISortableList<PlatformWinLossMember>, metaModel.SortBy, metaModel.SortOrder);
                AssignSheetData(sheet, templateSheet, data);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            HideColumn(sheet, metaModel);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private static void SortMemberData(ISortableList<PlatformWinLossMember> dataList, string sortBy, string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                var property = typeof(PlatformWinLossMember).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    dataList.Sort(property.Name, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(PlatformWinLossMember.Index), true);
                }
            }
        }

        private static string GetProductTitle(IList<ProductOption> productOptions)
        {
            if (productOptions.Count() == 1)
            {
                return $"{ViewResources.Product}: {productOptions.First().Code}";
            }

            var selectedProducts = productOptions.Where(x => x.Status);
            if (selectedProducts.Count() == productOptions.Count())
            {
                return $"{ViewResources.Product}: {ViewResources.All}";
            }

            var text = selectedProducts
                .Aggregate(string.Empty, (current, x) => current + (string.IsNullOrEmpty(current) ? x.Code : ", " + x.Code));

            return $"{ViewResources.Product}: {text}";
        }

        private static void SetTextRowHeader(IRow row, IEnumerable<string> headerTextList)
        {
            for (int i = 0; i < headerTextList.Count(); i++)
            {
                row.Cells[i].SetCellValue(headerTextList.ElementAt(i));
            }
        }

        private static void AssignCellData(IRow row, PlatformWinLossMember item)
        {
            row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(item.SystemId);
            row.Cells[ColumnTurnover].SetCellValue((double)item.Turnover);
            row.Cells[ColumnTotalTrans].SetCellValue(item.TotalTransactions);
            row.Cells[ColumnCustomerWl].SetCellValue((double)item.CustomerWinLoss);
            row.Cells[ColumnCustomerCommission].SetCellValue((double)item.CustomerComm);
            row.Cells[ColumnCustomerTotal].SetCellValue((double)item.CustomerTotal);
            row.Cells[ColumnLicenseeWl].SetCellValue((double)item.LicenseeWL);
            row.Cells[ColumnBaCommission].SetCellValue((double)item.BACommission);
            row.Cells[ColumnAmtDueToBa].SetCellValue((double)item.AmtDueToBA);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, PlatformWinLossMetaModel metaModel)
        {
            var platformText = ViewResources.Platform + ": " + DataHelper.GetPlatformName(metaModel.PlatformType);
            var currencyText = metaModel.CurrencyId == 0
                ? ViewResources.AllCurrencies
                : $"{ViewResources.Currency} {metaModel.CurrencyName}";

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

        private void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);
            var headerTextList = new[]
            {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Currency,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L,
                ViewResources.BACommission,
                ViewResources.AmtDueToBA
            };

            SetTextRowHeader(row, headerTextList);

            var secondRow = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate + 1, destRowIndex: RowHeaderTemplate + 1);
            secondRow.Cells[ColumnCustomerWl].SetCellValue(ViewResources.WinLoss);
            secondRow.Cells[ColumnCustomerCommission].SetCellValue(ViewResources.Comm);
            secondRow.Cells[ColumnCustomerTotal].SetCellValue(ViewResources.Total);
        }

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableResultModel<PlatformWinLossMember> data)
        {
            var rowIndex = 0;
            int index = 1;
            foreach (PlatformWinLossMember item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, rowIndex + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(index);
                row.Cells[ColumnCurrency].SetCellValue(item.CurrencyName);
                AssignCellData(row, item);
                rowIndex++;
                index++;
            }

            AssignTotal(sheet, templateSheet, data);
        }

        private void AssignTotal(ISheet sheet, ISheet templateSheet, SortableResultModel<PlatformWinLossMember> data)
        {
            var totalRow = data.DataList.Count() + RowContentTemplate;

            var row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, totalRow);
            _builder.MergeCells(sheet, firstRow: totalRow, lastRow: totalRow, firstCol: ColumnIndex, lastCol: ColumnCurrency);
            row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);
            AssignCellData(row, data.TotalData);
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo);
        }

        private void HideColumn(ISheet sheet, PlatformWinLossMetaModel metaModel)
        {
            if (!metaModel.HasComm)
            {
                _builder.HideColumn(sheet, ColumnCustomerTotal, ColumnCustomerCommission);
                _builder.HideRow(sheet, RowHeaderTemplate + 1);
            }

            if (!metaModel.ProductOptions.Any(x => x.Code == Product.BA && x.Status))
            {
                _builder.HideColumn(sheet, ColumnBaCommission, ColumnAmtDueToBa);
            }
        }
    }
}