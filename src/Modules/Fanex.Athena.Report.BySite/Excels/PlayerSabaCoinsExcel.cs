namespace Fanex.Athena.Report.BySite.Excels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using NPOI.SS.UserModel;

    public class PlayerSabaCoinsExcel : IPlayerSabaCoinsExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 1;
        private const int ColumnSystemId = 2;
        private const int ColumnOpeningBalance = 3;
        private const int ColumnReward_Promotion = 4;
        private const int ColumnExchangeToCash = 5;
        private const int ColumnTurnOver = 6;
        private const int ColumnPendingFunds = 7;
        private const int ColumnPlayerWL = 8;
        private const int ColumnClosingBalance = 9;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 10;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowGrantTotalTemplate = 5;

        private readonly IExcelBuilder _builder;

        public PlayerSabaCoinsExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<PlayerSabaCoins> data, PlayerSabaCoinsMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<PlayerSabaCoins> data, PlayerSabaCoinsMetaModel metaModel)
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
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.OpeningBalance,
                ViewResources.Reward_Promotion,
                ViewResources.ExchangeToCash,
                ViewResources.Turnover,
                ViewResources.PendingFunds,
                ViewResources.PlayerW_L,
                ViewResources.ClosingBalance
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<PlayerSabaCoins> data)
        {
            int index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                row.Cells[ColumnIndex].SetCellValue(++index);
                AssignCellData(row, item);
            }

            AssignGrandTotal(sheet, templateSheet, data, index + RowContentTemplate);
        }

        private static void AssignCellData(IRow row, PlayerSabaCoins item)
        {
            row.Cells[ColumnExternalId].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemId].SetCellValue(item.Username);
            row.Cells[ColumnOpeningBalance].SetCellValue((double)item.OpeningBalance);
            row.Cells[ColumnReward_Promotion].SetCellValue((double)item.RewardPromotionAmt);
            row.Cells[ColumnExchangeToCash].SetCellValue((double)item.ExchangeAmt);
            row.Cells[ColumnTurnOver].SetCellValue((double)item.Turnover);
            row.Cells[ColumnPendingFunds].SetCellValue((double)item.Outstanding);
            row.Cells[ColumnPlayerWL].SetCellValue((double)item.WinLoss);
            row.Cells[ColumnClosingBalance].SetCellValue((double)item.ClosingBalance);
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<PlayerSabaCoins> data, int index)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

            AssignCellData(row, data.TotalData);

            row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: ColumnSystemId);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, PlayerSabaCoinsMetaModel metaModel)
        {
            var reportTitle = $"{ViewResources.PlayerSabaCoins}, {metaModel.FromDate:MM/dd/yyyy} -> {metaModel.ToDate:MM/dd/yyyy}";

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