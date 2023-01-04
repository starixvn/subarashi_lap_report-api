namespace Fanex.Athena.Report.BySite.Excels
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources;
    using NPOI.SS.UserModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class AbnormalBetSummaryExcel : IAbnormalBetSummaryExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnSport = 2;
        private const int ColumnLeague = 3;
        private const int ColumnMatch = 4;
        private const int ColumnKOTime = 5;
        private const int ColumnLive = 6;
        private const int ColumnTotalTransaction = 7;
        private const int ColumnBetType = 8;
        private const int ColumnChoice = 9;
        private const int ColumnReason = 10;
        private const int ColumnFinalSocre = 11;

        private const int RowNoInfoTemplate = 5;
        private const int ColumnTemplateCount = 12;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 2;
        private const int RowEndContentTemplate = 4;

        private readonly IExcelBuilder _builder;

        public AbnormalBetSummaryExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<AbnormalBetSummary> data, AbnormalBetMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<AbnormalBetSummary> data, AbnormalBetMetaModel metaModel)
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
                ViewResources.Date,
                ViewResources.Sport,
                ViewResources.League,
                ViewResources.Match,
                ViewResources.KOTime,
                ViewResources.Live,
                ViewResources.TotalTransaction,
                ViewResources.BetType,
                ViewResources.Choice,
                ViewResources.Reason,
                ViewResources.FinalScore
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<AbnormalBetSummary> data)
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

        private static void AssignCellData(IRow row, AbnormalBetSummary item)
        {
            row.Cells[ColumnDate].SetCellValue(item.Date);
            row.Cells[ColumnSport].SetCellValue(item.Sport);
            row.Cells[ColumnLeague].SetCellValue(item.League);
            row.Cells[ColumnMatch].SetCellValue(item.Match);
            row.Cells[ColumnKOTime].SetCellValue(item.KOTime);
            row.Cells[ColumnLive].SetCellValue(item.Live);
            row.Cells[ColumnTotalTransaction].SetCellValue((double)item.TotalTransaction);
            row.Cells[ColumnBetType].SetCellValue(item.BetType);
            row.Cells[ColumnChoice].SetCellValue(item.Choice);
            row.Cells[ColumnReason].SetCellValue(item.Reason);
            row.Cells[ColumnFinalSocre].SetCellValue(item.FinalScore);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, AbnormalBetMetaModel metaModel)
        {
            var reportTitle = $"{ViewResources.AbnormalBet}, {ViewResources.ViewinSummary}, {metaModel.FromDate:MM/dd/yyyy} -> {metaModel.ToDate:MM/dd/yyyy}";

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