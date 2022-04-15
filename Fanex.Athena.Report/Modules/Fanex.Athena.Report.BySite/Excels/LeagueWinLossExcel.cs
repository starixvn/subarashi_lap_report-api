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

    public class LeagueWinLossExcel : ILeagueWinLossExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";
        private const int Parlay = 0;

        private const int ColumnIndex = 0;
        private const int ColumnSport = 1;
        private const int ColumnLeague = 2;
        private const int ColumnMatch = 3;
        private const int ColumnBetType = 4;
        private const int ColumnBetCount = 5;
        private const int ColumnTurnover = 6;
        private const int ColumnCustomerWl = 7;
        private const int ColumnLicenseeWl = 8;
        private const int ColumnBAComm = 9;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 10;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowGrantTotalTemplate = 5;

        private readonly IExcelBuilder _builder;

        public LeagueWinLossExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<LeagueWinLoss> data, LeagueWinLossMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<LeagueWinLoss> data, LeagueWinLossMetaModel metaModel)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet, metaModel, data);

            if (data.DataList.Any())
            {
                SortMemberData(data, metaModel);
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

        private static void SortMemberData(SortableReportResultModel<LeagueWinLoss> data, LeagueWinLossMetaModel metaModel)
        {
            if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.League)
            {
                var parlayData = data.DataList.FirstOrDefault(_ => _.SportType == Parlay);

                if (parlayData != null)
                {
                    data.DataList.Remove(parlayData);
                    data.DataList.Add(parlayData);
                }
            }
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate, destRowIndex: RowHeaderTemplate);

            var headerTextList = new[]
            {
                "#",
                ViewResources.Sport,
                ViewResources.League,
                ViewResources.Match,
                ViewResources.BetType,
                ViewResources.BetCount,
                ViewResources.Turnover,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L,
                ViewResources.BACommission,
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<LeagueWinLoss> data)
        {
            int index = 0;
            var rowIndex = 1;

            foreach (var item in data.DataList)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                AssignCellData(row, item);

                foreach (var detail in item.Detail)
                {
                    var detailRow = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);
                    detailRow.Cells[ColumnIndex].SetCellValue(rowIndex++);
                    AssignCellData(detailRow, detail, item.SportName);
                    index++;
                }
            }

            AssignGrandTotal(sheet, templateSheet, data, index + RowContentTemplate);
        }

        private static void AssignCellData(IRow row, LeagueWinLoss item, string sportName = "")
        {
            row.Cells[ColumnSport].SetCellValue(sportName);
            row.Cells[ColumnLeague].SetCellValue(item.LeagueName);
            row.Cells[ColumnMatch].SetCellValue(item.MatchName);
            row.Cells[ColumnBetType].SetCellValue(item.BetTypeName);
            row.Cells[ColumnBetCount].SetCellValue(item.BetCount);
            row.Cells[ColumnTurnover].SetCellValue((double)item.TurnOver);
            row.Cells[ColumnCustomerWl].SetCellValue((double)item.CustomerWL);
            row.Cells[ColumnLicenseeWl].SetCellValue((double)item.LicenseeWL);
            row.Cells[ColumnBAComm].SetCellValue((double)item.BAComm);
        }

        private void AssignGrandTotal(ISheet sheet, ISheet templateSheet, SortableReportResultModel<LeagueWinLoss> data, int index)
        {
            const int mergedColumn = 4;
            var row = _builder.CopyRow(sheet, templateSheet, RowGrantTotalTemplate, index);

            AssignCellData(row, data.TotalData);

            row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);

            _builder.MergeCells(sheet, firstRow: index, lastRow: index, firstCol: ColumnIndex, lastCol: mergedColumn);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, LeagueWinLossMetaModel metaModel, SortableReportResultModel<LeagueWinLoss> data)
        {
            var currencyText = $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            var baseCurrency = !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName;
            var baseCurrencyText = $"{ViewResources.BaseCurrency}: {baseCurrency}";
            var extendInfo = "";
            var userNameText = "";
            if (!string.IsNullOrWhiteSpace(metaModel.UserName)){
                userNameText += $", {ViewResources.Username}: {metaModel.UserName}";
            }

            if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.Match && data != null && data.DataList.Any())
            {
                extendInfo = $", {data.DataList.FirstOrDefault().LeagueName}";
            }

            if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.Bettype && data != null && data.DataList.Any())
            {
                extendInfo = $", {data.DataList.FirstOrDefault().MatchName}";
            }


            var reportTitle = string.Format(
                                         ViewResources.LeagueWinLossTitle,
                                         metaModel.FromDate.ToString("MM/dd/yyyy"),
                                         metaModel.ToDate.ToString("MM/dd/yyyy"),
                                         currencyText,
                                         baseCurrencyText) + userNameText + (string.IsNullOrEmpty(extendInfo) ? "" : extendInfo);

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

        private void HideColumns(ISheet sheet, LeagueWinLossMetaModel metaModel)
        {
            if (!metaModel.ShowBA)
            {
                _builder.HideColumn(sheet, ColumnBAComm);
            }

            switch (metaModel.DrillDownLevel)
            {
                case (int)LeagueWinLossReportLevel.League:
                    _builder.HideColumn(sheet, ColumnMatch, ColumnBetType);
                    break;

                case (int)LeagueWinLossReportLevel.Match:
                    _builder.HideColumn(sheet, ColumnSport, ColumnLeague, ColumnBetType);
                    break;

                default:
                    _builder.HideColumn(sheet, ColumnSport, ColumnMatch, ColumnLeague);
                    break;
            }
        }
    }
}