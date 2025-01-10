using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Resources;
using NPOI.SS.UserModel;
using System.IO;
using System.Linq;

namespace Fanex.Athena.Report.BySite.Excels
{
    public class WeeklyLiveMatchExcel : IWeeklyLiveMatchExcel
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        public const string FormatDate = "MM/dd/yyyy";

        public const int RowTitleTemplate = 0;
        public const int RowHeaderTemplate = 1;
        protected const int ColumnIndex = 0;

        private const int ColumnSport = 1;
        private const int ColumnTemplateCount = 12;
        private const int NumberOfStaticColumns = 2; // Column: Index, Sport.
        private const int MaxWeekColumns = 10;

        private const int RowContentTemplate = 2;
        private const int RowTotalIndex = 3;
        private const int RowNoInfoTemplate = 4;

        private readonly IExcelBuilder _builder;
        private WeeklyLiveMatchResultModel _data;
        private WeeklyLiveMatchOptions _options;

        public WeeklyLiveMatchExcel(IExcelBuilder builder)
        {
            _builder = builder;
        }

        public byte[] Export(string template, WeeklyLiveMatchResultModel data, WeeklyLiveMatchOptions options)
        {
            _data = data;
            _options = options;

            var workBook = GetWorkbook(template);
            using var stream = new MemoryStream();
            workBook.Write(stream);
            workBook.Close();

            return stream.ToArray();
        }

        private IWorkbook GetWorkbook(string template)
        {
            var workBook = _builder.LoadTemplate(template);
            var sheet = _builder.CloneSheet(workBook, TemplateSheetName, SheetName);
            var templateSheet = workBook.GetSheet(TemplateSheetName);

            AssignHeader(sheet, templateSheet);
            AssignTitle(sheet, templateSheet);

            if (_data.DataList.Any())
            {
                AssignSheetData(sheet, templateSheet);
            }
            else
            {
                AssignNoInfoRow(sheet, templateSheet);
            }

            ProcessUi(sheet, templateSheet);
            _builder.RemoveSheet(workBook, TemplateSheetName);

            return workBook;
        }

        private void AssignHeader(ISheet sheet, ISheet templateSheet)
        {
            var row = _builder.CopyRow(sheet, templateSheet, RowHeaderTemplate, RowHeaderTemplate);

            row.Cells[ColumnSport].SetCellValue(ViewResources.Sport);

            var index = ColumnSport + 1;
            foreach (var header in _options.WeekHeaders)
            {
                row.Cells[index++].SetCellValue(header);
            }
        }

        private void AssignNoInfoRow(ISheet sheet, ISheet templateSheet)
        {
            _builder.SetRowValue(sheet, templateSheet, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount,
                                 ViewResources.NoInfo);
        }

        private void AssignSheetData(ISheet sheet, ISheet templateSheet)
        {
            int index;
            var rowCount = _data.DataList.Count();

            for (index = 0; index < rowCount; index++)
            {
                var dataItem = _data.DataList.ElementAt(index);

                var row = _builder.CopyRow(sheet, templateSheet, RowContentTemplate, index + RowContentTemplate);

                row.Cells[ColumnIndex].SetCellValue(dataItem.Index);
                row.Cells[ColumnSport].SetCellValue(dataItem.SportName);

                var weekColumnIndex = ColumnSport + 1;
                for (var i = 0; i < _data.NumberOfWeeks; i++)
                {
                    row.Cells[weekColumnIndex++].SetCellValue(dataItem.NumbersOfMatches.ElementAt(i));
                }
            }
        }

        private void AssignTitle(ISheet sheet, ISheet templateSheet)
        {
            var fromDate = _options.MetaModel.FromDate.ToString(FormatDate);
            var toDate = _options.MetaModel.ToDate.ToString(FormatDate);

            var reportTitle = string.Format(ViewResources.WeeklyLiveMatches + ", {0} --> {1}", fromDate, toDate);

            _builder.SetRowValue(
                                 sheet,
                                 templateSheet,
                                 RowTitleTemplate,
                                 RowTitleTemplate,
                                 ColumnTemplateCount,
                                 reportTitle);

            sheet.CreateFreezePane(0, RowContentTemplate);

            _builder.ClearTemplate(sheet, RowContentTemplate);
        }

        private void ProcessUi(ISheet sheet, ISheet templateSheet)
        {
            AssignTotalRow(sheet, templateSheet, _data.DataList.Count() + RowContentTemplate);
            var hiddenColumns = Enumerable.Range(_data.NumberOfWeeks + NumberOfStaticColumns,
                                                 MaxWeekColumns - _data.NumberOfWeeks);
            _builder.HideColumn(sheet, hiddenColumns.ToArray());
        }

        private void AssignTotalRow(ISheet sheet, ISheet templateSheet, int rowIndex)
        {
            var cellIndex = 2;

            var row = _builder.CopyRow(sheet, templateSheet, RowTotalIndex, rowIndex);

            _builder.MergeCols(sheet, ColumnIndex, ColumnSport, rowIndex);
            row.Cells[ColumnIndex].SetCellValue(ViewResources.Total);

            for (var i = 0; i < _data.NumberOfWeeks; i++)
            {
                row.Cells[cellIndex++].SetCellValue(_data.WeeksTotalData[i]);
            }
        }
    }
}
