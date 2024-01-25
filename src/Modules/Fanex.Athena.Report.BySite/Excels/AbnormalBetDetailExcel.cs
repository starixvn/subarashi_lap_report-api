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

    public class AbnormalBetDetailExcel : IAbnormalBetDetailExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalID = 2;
        private const int ColumnSystemID = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnRefNo = 5;
        private const int ColumnOdds = 6;
        private const int ColumnStake = 7;
        private const int ColumnHDP = 8;
        private const int ColumnLiveHomeScore = 9;
        private const int ColumnLiveAwayScore = 10;
        private const int ColumnSport = 11;
        private const int ColumnBetType = 12;
        private const int ColumnChoice = 13;
        private const int ColumnTransTime = 14;
        private const int ColumnLeague = 15;
        private const int ColumnMatch = 16;
        private const int ColumnKOTime = 17;
        private const int ColumnLive = 18;
        private const int ColumnFinalScore = 19;
        private const int ColumnIP = 20;
        private const int ColumnLocation = 21;

        private const int RowNoInfoTemplate = 5;
        private const int ColumnTemplateCount = 22;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 2;
        private const int RowEndContentTemplate = 4;

        private readonly IExcelBuilder _builder;

        public AbnormalBetDetailExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<AbnormalBetDetail> data, AbnormalBetMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<AbnormalBetDetail> data, AbnormalBetMetaModel metaModel)
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

            sheet.CreateFreezePane(ColumnOdds, RowContentTemplate);

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
                ViewResources.Currency,
                ViewResources.RefNo,
                ViewResources.Odds,
                ViewResources.Stake,
                ViewResources.HDP,
                ViewResources.Score,
                ViewResources.Score,
                ViewResources.Sport,
                ViewResources.BetType,
                ViewResources.Choice,
                ViewResources.TransTime,
                ViewResources.League,
                ViewResources.Match,
                ViewResources.KOTime,
                ViewResources.Live,
                ViewResources.FinalScore,
                ViewResources.IP,
                ViewResources.Location
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<AbnormalBetDetail> data)
        {
            int index = 0;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, RowContentTemplate + index);
                row.Cells[ColumnIndex].SetCellValue(++index);
                AssignCellData(row, item);
            }

            _builder.CopyRow(sheet, templateSheet, RowEndContentTemplate, RowContentTemplate + index);
        }

        private static void AssignCellData(IRow row, AbnormalBetDetail item)
        {
            row.Cells[ColumnDate].SetCellValue(item.Date);
            row.Cells[ColumnExternalID].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemID].SetCellValue(item.SystemID);
            row.Cells[ColumnCurrency].SetCellValue(item.Currency);
            row.Cells[ColumnRefNo].SetCellValue(item.RefNo);
            row.Cells[ColumnOdds].SetCellValue((double)item.Odds);
            row.Cells[ColumnStake].SetCellValue((double)item.Stake);
            row.Cells[ColumnHDP].SetCellValue((double)item.HDP);
            row.Cells[ColumnLiveHomeScore].SetCellValue(item.LiveHomeScore);
            row.Cells[ColumnLiveAwayScore].SetCellValue(item.LiveAwayScore);
            row.Cells[ColumnSport].SetCellValue(item.Sport);
            row.Cells[ColumnBetType].SetCellValue(item.BetType);
            row.Cells[ColumnChoice].SetCellValue(item.Choice);
            row.Cells[ColumnTransTime].SetCellValue(item.TransTime);
            row.Cells[ColumnLeague].SetCellValue(item.League);
            row.Cells[ColumnMatch].SetCellValue(item.Match);
            row.Cells[ColumnKOTime].SetCellValue(item.KOTime);
            row.Cells[ColumnLive].SetCellValue(item.Live);
            row.Cells[ColumnFinalScore].SetCellValue(item.FinalScore);
            row.Cells[ColumnIP].SetCellValue(item.IP);
            row.Cells[ColumnLocation].SetCellValue(item.Location);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, AbnormalBetMetaModel metaModel)
        {
            var currencyText = $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            var baseCurrency = !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName;
            var baseCurrencyText = metaModel.IsConvertAllToBaseCurrency ? $", {ViewResources.BaseCurrency}: {baseCurrency}" : "";
            var userText = string.IsNullOrEmpty(metaModel.UserName) ? "" : $", {ViewResources.Username}: {metaModel.UserName}";
            var reportTitle = $"{ViewResources.AbnormalBet}, {ViewResources.ViewinDetail}, " +
                $"{metaModel.FromDate:MM/dd/yyyy} -> {metaModel.ToDate:MM/dd/yyyy}, {currencyText}" + baseCurrencyText + userText;

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