namespace Fanex.Athena.Report.ByPlayer.Excels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common.Excel;
    using Common.Excel.Interfaces;
    using Common.Model.ViewModel;
    using DTO;
    using Interfaces;
    using NPOI.SS.UserModel;
    using Options;
    using Resources;

    public class CustomerPendingFundsExcel : ICustomerPendingFundsExcel
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        public const int RowTitleTemplate = 0;

        public const int RowHeaderTemplate = 1;

        private const int RowContentTemplate = 2;

        private const int RowTotalTemplate = 3;

        private const int RowNoInfoTemplate = 4;

        private const int ColumnIndex = 0;

        private const int ColumnExternalId = 1;

        private const int ColumnSystemId = 2;

        private const int ColumnBetCount = 3;

        private const int ColumnPendingFunds = 4;

        private const int ColumnTemplateCount = 5;

        private readonly IExcelBuilder _builder;

        private readonly string[] _headerTextList =
        {
            "#",
            ViewResources.ExternalId,
            ViewResources.SystemId,
            ViewResources.BetCounts,
            ViewResources.PendingFunds
        };

        private readonly int PageSize = 30000;

        private IEnumerable<CustPendingFunds> _data;

        private CustomerPendingFundsExcelOptions _options;

        private CustPendingFunds _totalData;

        public CustomerPendingFundsExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(
            string template,
            ReportResultModel<CustPendingFunds> data,
            CustomerPendingFundsExcelOptions excelOption)
        {
            _data = data.DataList;
            _totalData = data.TotalData;
            _options = excelOption;

            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        protected void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);

            ExcelHelper.SetTextRowHeader(row, _headerTextList);
        }

        protected void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount,
                ViewResources.NoInfo);
        }

        protected void AssignSheetData(ISheet sheet, ISheet templateSheet, int pageIndex)
        {
            var rowCount = Math.Min(_data.Count(), checked((pageIndex + 1) * PageSize));
            var rowIndex = RowContentTemplate;

            for (var index = pageIndex * PageSize; index < rowCount; index++)
            {
                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, rowIndex);

                var rowData = _data.ElementAt(index);

                row.Cells[ColumnIndex].SetCellValue(index + 1);
                row.Cells[ColumnExternalId].SetCellValue(rowData.ExternalId);
                row.Cells[ColumnSystemId].SetCellValue(rowData.SystemId);
                row.Cells[ColumnBetCount].SetCellValue(rowData.BetCounts);
                row.Cells[ColumnPendingFunds].SetCellValue((double)rowData.PendingFunds);

                rowIndex++;
            }

            AssignSumData(sheet, templateSheet, rowIndex);
        }

        protected void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var reportTitle = string.Format(ViewResources.CustomerPedingFundsTitle,
                ExcelHelper.GetTitleCurrencyName(_options));

            _builder.SetRowValue(sheet,
                templateSheet,
                RowTitleTemplate,
                RowTitleTemplate,
                _headerTextList.Length,
                reportTitle);

            // Remove content below header before insert new data
            _builder.ClearTemplate(sheet, RowContentTemplate);

            // Freeze title and header
            sheet.CreateFreezePane(0, RowContentTemplate);
        }

        protected virtual void AssignSumData(ISheet sheet, ISheet templateSheet, int totalRow)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowTotalTemplate, totalRow);

            row.Cells[ColumnBetCount].SetCellValue(_totalData.BetCounts);
            row.Cells[ColumnPendingFunds].SetCellValue((double)_totalData.PendingFunds);
            row.Cells[0].SetCellValue(ViewResources.Total);

            _builder.MergeCols(sheet, ColumnIndex, ColumnSystemId, totalRow);
        }

        protected void ProcessUi(ISheet sheet)
        {
            if (_options.SystemIdVisible)
            {
                _builder.HideColumn(sheet, ColumnSystemId);
            }
        }

        private IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            if (_data.Any())
            {
                var numberOfSheets = ExcelHelper.GetNumberOfSheets(_data.Count(), PageSize);

                for (var i = 0; i < numberOfSheets; i++)
                {
                    var sheet = _builder.CloneSheet(workBook, TemplateSheetName, "Data" + (i + 1));

                    AssignTitle(sheet, templateSheet);
                    AssignHeader(sheet, templateSheet);
                    AssignSheetData(sheet, templateSheet, i);
                    ProcessUi(sheet);
                }
            }
            else
            {
                var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);

                AssignTitle(sheet, templateSheet);
                AssignHeader(sheet, templateSheet);
                AssignNoInfoRow(sheet, templateSheet);
                ProcessUi(sheet);
            }

            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }
    }
}