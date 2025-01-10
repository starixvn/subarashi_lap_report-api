using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class GodOfFortuneWinLossExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnTurnover = 5;
        private const int ColumnDiscountValue = 6;
        private const int ColumnTotalTrans = 7;
        private const int ColumnCustomerWl = 8;
        private const int ColumnLicenseeWl = 9;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 10;
        private const int RowTotalNoSolidBorder = 5;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;
        private const int RowGrantTotalTemplate = 6;

        private const string TemplatePath = "TemplatePath";
        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<ISheet> _sheet;
        private Mock<ISheet> _templateSheet;

        [Theory]
        [InlineData((int)ReportLevel.Main)]
        [InlineData((int)ReportLevel.Licensee)]
        [InlineData((int)ReportLevel.Customer)]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows(int level)
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
              .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>() { new GodOfFortuneWinLoss { LicenseeWinLoss = 100, CustomerWinLoss = 200 } })
              .Create();
            var metaModel = _fixture.Build<GodOfFortuneWinLossMetaModel>().With(_ => _.DrillDownLevel, level).Create();

            var reportTitle = CreateTitle(metaModel);

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, reportTitle), Times.Once);
        }

        [Fact]
        public void AssignHeader_WhenCall_AssignCorrectValueForHeaderRows()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();
            var headerTextList = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Currency,
                ViewResources.Turnover,
                ViewResources.DiscountValue,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L
            };

            var data = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
                .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>())
                .Create();
            var metaModel = _fixture.Create<GodOfFortuneWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var firstHeaderRow = rows[0];

            // Assert
            ExcelTestHelper.AssertSetTextRowHeader(firstHeaderRow.Object, headerTextList);
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
               .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>())
               .Create();
            var metaModel = _fixture.Create<GodOfFortuneWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var bodyData = _fixture.Build<GodOfFortuneWinLoss>()
                .Create();
            var data = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
                .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>() { bodyData })
                .Create();
            var metaModel = _fixture.Create<GodOfFortuneWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[2];

            // Assert
            AssertRowData(bodyRow, bodyData);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var secondHeaderRow = rowList[1];
            var bodyRow = rowList[2];
            var totalRow = rowList[3];
            var grandTotalRow = rowList[4];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowTotalTemplate || x == RowTotalNoSolidBorder), It.IsAny<int>()))
                .Returns(totalRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowGrantTotalTemplate, It.IsAny<int>()))
               .Returns(grandTotalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate + 1);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTotalTemplate);
            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowGrantTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, totalRow, grandTotalRow };
        }

        private GodOfFortuneWinLossExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new GodOfFortuneWinLossExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(GodOfFortuneWinLossMetaModel metaModel)
        {
            var baseCurrencyText = string.Format(
                ViewResources.Viewin,
                !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName);

            var isMainLevel = metaModel.DrillDownLevel == (int)ReportLevel.Main;
            var fromdate = isMainLevel ? metaModel.FromDate : metaModel.DrillDownFromDate;
            var todate = isMainLevel ? metaModel.ToDate : metaModel.DrillDownToDate;

            return string.Format(
                                         ViewResources.GodOfFortuneWinLossTitle,
                                         fromdate.ToString("MM/dd/yyyy"),
                                         todate.ToString("MM/dd/yyyy"),
                                         baseCurrencyText);
        }

        private void AssertRowData(Mock<IRow> row, GodOfFortuneWinLoss dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(1), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCurrency]).Verify(c => c.SetCellValue(dataItem.CurrencyName), Times.Once);
            AssertCommonCellData(row, dataItem);
        }

        private void AssertCommonCellData(Mock<IRow> row, GodOfFortuneWinLoss dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(dataItem.ExternalId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(dataItem.SystemId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnover]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.TurnOver)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTotalTrans]).Verify(c => c.SetCellValue(dataItem.TotalTransaction), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWinLoss)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicenseeWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.LicenseeWinLoss)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnDiscountValue]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Discount)), Times.Once);
        }
    }
}