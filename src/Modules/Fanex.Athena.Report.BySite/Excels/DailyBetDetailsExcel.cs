namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using NPOI.SS.UserModel;

    public class DailyBetDetailsExcel : IDailyBetDetailsExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnSettlementDate = 0;
        private const int ColumnBrandUrl = 1;
        private const int ColumnGameType = 2;
        private const int ColumnCurrencyName = 3;
        private const int ColumnBetAmount = 4;
        private const int ColumnCreditAmount = 5;
        private const int ColumnNetAmount = 6;
        private const int ColumnRefNo = 7;
        private const int ColumnIndex = 8;

        private const int RowNoInfoTemplate = 5;
        private const int ColumnTemplateCount = 9;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;

        private readonly IExcelBuilder _builder;

        public DailyBetDetailsExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, ResultModel<DailyBetDetails> data, DailyBetDetailsMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, ResultModel<DailyBetDetails> data, DailyBetDetailsMetaModel metaModel)
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

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);

            var headerTextList = new[]
            {
                "settlement_date",
                "brand_url",
                "game_type",
                "currency_code",
                "bet_amount",
                "credit_amount",
                "net_amount",
                "ref_no",
                "#"
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, ResultModel<DailyBetDetails> data)
        {
            var index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                ++index;
                AssignCellData(row, item);
            }
        }

        private static void AssignCellData(IRow row, DailyBetDetails item)
        {
            row.Cells[ColumnSettlementDate].SetCellValue(item.SettlementDateString);
            row.Cells[ColumnBrandUrl].SetCellValue(item.BrandUrl);
            row.Cells[ColumnGameType].SetCellValue(item.GameType);
            row.Cells[ColumnCurrencyName].SetCellValue(item.CurrencyName);
            row.Cells[ColumnBetAmount].SetCellValue((double)item.BetAmount);
            row.Cells[ColumnCreditAmount].SetCellValue((double)item.CreditAmount);
            row.Cells[ColumnNetAmount].SetCellValue((double)item.NetAmount);
            row.Cells[ColumnRefNo].SetCellValue(item.RefNo);
            row.Cells[ColumnIndex].SetCellValue(item.RIdx);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, DailyBetDetailsMetaModel metaModel)
        {
            var currencyIdSelectedList = metaModel.CurrencyIdList.Split(',').Select(int.Parse);
            var displayedCurrency = metaModel.CurrencyList.Count == currencyIdSelectedList.Count()
                ? ViewResources.AllCurrencies
                : $"{ViewResources.Currency}: {string.Join(",", metaModel.CurrencyList.Where(_ => currencyIdSelectedList.Contains(_.Key)).Select(_ => _.Value))}";

            var reportTitle = $"{ViewResources.DailyBetDetails}, GMT+8, {metaModel.Date:dd/MM/yyyy} - {displayedCurrency}";

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