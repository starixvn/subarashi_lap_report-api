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

    public class AbnormalBetIpSummaryExcel : IAbnormalBetIpSummaryExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnExternalID = 1;
        private const int ColumnSystemID = 2;
        private const int ColumnIp = 3;
        private const int ColumnLocation = 4;
        private const int ColumnTicketIp = 5;
        private const int ColumnTicketLocation = 6;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 7;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 3;
        private const int RowEndContentTemplate = 5;

        private readonly IExcelBuilder _builder;

        public AbnormalBetIpSummaryExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, SortableReportResultModel<AbnormalBetIpSummary> data, AbnormalBetMetaModel metaModel)
        {
            var workBook = GetWorkbook(template, data, metaModel);

            using (var stream = new MemoryStream())
            {
                workBook.Write(stream);
                workBook.Close();

                return stream.ToArray();
            }
        }

        public IWorkbook GetWorkbook(string template, SortableReportResultModel<AbnormalBetIpSummary> data, AbnormalBetMetaModel metaModel)
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
                ViewResources.LastLogin,
                ViewResources.LastLogin,
                ViewResources.Ticket,
                ViewResources.Ticket
            };

            SetTextRowHeader(row, headerTextList);

            var row2 = _builder.CopyRow(sheet, templateSheet, sourceRowIndex: RowHeaderTemplate + 1, destRowIndex: RowHeaderTemplate + 1);

            row2.Cells[ColumnIp].SetCellValue(ViewResources.IP);
            row2.Cells[ColumnLocation].SetCellValue(ViewResources.Location);
            row2.Cells[ColumnTicketIp].SetCellValue(ViewResources.IP);
            row2.Cells[ColumnTicketLocation].SetCellValue(ViewResources.Location);
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

        private void AssignSheetData(ISheet sheet, ISheet templateSheet, SortableReportResultModel<AbnormalBetIpSummary> data)
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

        private static void AssignCellData(IRow row, AbnormalBetIpSummary item)
        {
            row.Cells[ColumnExternalID].SetCellValue(item.ExternalId);
            row.Cells[ColumnSystemID].SetCellValue(item.SystemID);
            row.Cells[ColumnIp].SetCellValue(item.LastLoginIP);
            row.Cells[ColumnLocation].SetCellValue(item.LastLoginLocation);
            row.Cells[ColumnTicketIp].SetCellValue(item.TicketIP);
            row.Cells[ColumnTicketLocation].SetCellValue(item.TicketLocation);
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet, AbnormalBetMetaModel metaModel)
        {
            var reportTitle = $"{ViewResources.AbnormalBet}, {ViewResources.ViewinIPSummary}, {metaModel.FromDate:MM/dd/yyyy} -> {metaModel.ToDate:MM/dd/yyyy}";

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