using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Resources;
using Moq;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class WeeklyLiveMatchExcelTest
    {
        private const string TemplatePath = "TemplatePath";

        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        public const int RowTitleTemplate = 0;

        public const int RowHeaderTemplate = 1;

        protected const int ColumnIndex = 0;

        private const int ColumnTemplateCount = 12;

        private const int NumberOfStaticColumns = 2; // Column: Index, Sport.

        private const int RowContentTemplate = 2;

        private const int RowTotalIndex = 3;

        private const int RowNoInfoTemplate = 4;

        private const int RowHeaderMockIndex = 0;

        private const int RowContentMockIndex = 1;

        private const int RowTotalMockIndex = 2;

        private readonly WeeklyLiveMatchExcel _excel;

        private readonly IFixture _fixture = new Fixture();

        private readonly IList<Mock<IRow>> _rowsMock;

        private Mock<IExcelBuilder> _excelBuilder;

        private Mock<ISheet> _sheet;

        private Mock<ISheet> _templateSheet;

        public WeeklyLiveMatchExcelTest()
        {
            _excel = CreateExcel();
            _rowsMock = CreateRows();
        }

        private WeeklyLiveMatchExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();

            _templateSheet = new Mock<ISheet>();
            var workbook = new Mock<IWorkbook>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName))
                         .Returns(_sheet.Object);

            return new WeeklyLiveMatchExcel(_excelBuilder.Object);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var headerRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            _excelBuilder
               .Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
               .Returns(headerRow.Object);

            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object,
                              It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate),
                              RowContentTemplate))
                         .Returns(bodyRow.Object);

            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTotalIndex);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotalIndex,
                              It.Is<int>(x => x >= RowContentTemplate)))
                         .Returns(grandTotalRow.Object);

            return new List<Mock<IRow>> { headerRow, bodyRow, grandTotalRow, };
        }

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows()
        {
            // Arrange
            var options = _fixture.Create<WeeklyLiveMatchOptions>();
            var fromDate = options.MetaModel.FromDate.ToString(WeeklyLiveMatchExcel.FormatDate);
            var toDate = options.MetaModel.ToDate.ToString(WeeklyLiveMatchExcel.FormatDate);

            var data = _fixture.Build<WeeklyLiveMatchResultModel>().With(x => x.DataList, new List<WeeklyLiveMatch>())
                               .With(x => x.NumberOfWeeks, options.WeekHeaders.Count).Create();

            var reportTitle = string.Format(ViewResources.WeeklyLiveMatches + ", {0} --> {1}", fromDate, toDate);

            // Action
            _excel.Export(TemplatePath, data, options);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount,
                reportTitle), Times.Once);
        }

        [Fact]
        public void AssignHeader_WhenCall_AssignCorrectValueForHeaderRows()
        {
            // Arrange
            var options = _fixture.Create<WeeklyLiveMatchOptions>();
            var data = _fixture.Build<WeeklyLiveMatchResultModel>().With(x => x.DataList, new List<WeeklyLiveMatch>())
                               .With(x => x.NumberOfWeeks, options.WeekHeaders.Count).Create();

            var headerTextList = new List<string>
            {
                "#",
                ViewResources.Sport
            };
            headerTextList.AddRange(options.WeekHeaders);

            // Action
            _excel.Export(TemplatePath, data, options);
            var firstHeaderRow = _rowsMock[RowHeaderMockIndex].Object;

            // Assert
            AssertSetTextRowHeader(firstHeaderRow, headerTextList, 1);
        }

        private static void AssertSetTextRowHeader(IRow row, IList<string> headers, int startIndex = 0)
        {
            for (var i = startIndex; i < headers.Count; i++)
            {
                var mockCell = Mock.Get(row.Cells[i]);
                mockCell.Verify(c => c.SetCellValue(headers[i]), Times.Once);
            }
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var options = _fixture.Create<WeeklyLiveMatchOptions>();
            var data = _fixture.Build<WeeklyLiveMatchResultModel>().With(x => x.DataList, new List<WeeklyLiveMatch>())
                               .With(x => x.NumberOfWeeks, options.WeekHeaders.Count).Create();

            // Action
            _excel.Export(TemplatePath, data, options);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount,
                ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var rowData = _fixture.Create<WeeklyLiveMatch>();
            var options = _fixture.Create<WeeklyLiveMatchOptions>();
            var data = _fixture.Build<WeeklyLiveMatchResultModel>()
                               .With(x => x.DataList, new List<WeeklyLiveMatch> { rowData, })
                               .With(x => x.NumberOfWeeks, options.WeekHeaders.Count).Create();

            // Action
            _excel.Export(TemplatePath, data, options);
            var contentRow = _rowsMock[RowContentMockIndex];

            // Assert
            AssertRowValue(contentRow, rowData.NumbersOfMatches.ToArray());
        }

        [Fact]
        public void AssignGrandTotal_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var options = _fixture.Create<WeeklyLiveMatchOptions>();
            var data = _fixture.Build<WeeklyLiveMatchResultModel>().With(x => x.DataList, new List<WeeklyLiveMatch>())
                               .With(x => x.NumberOfWeeks, options.WeekHeaders.Count).Create();

            // Action
            _excel.Export(TemplatePath, data, options);
            var totalRow = _rowsMock[RowTotalMockIndex];

            // Assert
            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
            AssertRowValue(totalRow, data.WeeksTotalData);
        }

        private static void AssertRowValue(IMock<IRow> row, IReadOnlyList<int> data)
        {
            var count = data.Count;

            for (var i = 0; i < count; i++)
            {
                Mock.Get(row.Object.Cells[NumberOfStaticColumns + i]).Verify(c => c.SetCellValue(data[i]), Times.Once);
            }
        }
    }
}
