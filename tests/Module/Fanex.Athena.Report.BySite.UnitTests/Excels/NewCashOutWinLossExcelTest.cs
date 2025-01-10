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
using System.Linq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class NewCashOutWinLossExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnTurnOver = 5;
        private const int ColumnTotalTransaction = 6;
        private const int ColumnCustomerWinLoss = 7;
        private const int ColumnCustomerComm = 8;
        private const int ColumnCustomerTotal = 9;
        private const int ColumnLicenseeWL = 10;
        private const int ColumnBaComm = 11;
        private const int ColumnAmtDueToBA = 12;

        private const int RowNoInfoTemplate = 7;
        private const int ColumnTemplateCount = 13;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalCurrencyTemplate = 4;
        private const int RowTotalCurrencyNoSolidBorder = 5;
        private const int RowGrantTotalTemplate = 6;

        private const string TemplatePath = "TemplatePath";
        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<ISheet> _sheet;
        private Mock<ISheet> _templateSheet;

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                .With(x => x.DataList, new SortableList<CashOutWinLoss>())
                .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

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

            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                .With(x => x.DataList, new SortableList<CashOutWinLoss>())
                .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var firstHeaderRow = rows[0];
            var secondHeaderRow = rows[1];

            // Assert
            Mock.Get(firstHeaderRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue("#"), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnDate]).Verify(c => c.SetCellValue(ViewResources.Date), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(ViewResources.ExternalId), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(ViewResources.SystemId), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnCurrency]).Verify(c => c.SetCellValue(ViewResources.Currency), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnTurnOver]).Verify(c => c.SetCellValue(ViewResources.Turnover), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnTotalTransaction]).Verify(c => c.SetCellValue(ViewResources.TotalTransaction), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnCustomerWinLoss]).Verify(c => c.SetCellValue(ViewResources.CustomerW_L), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnLicenseeWL]).Verify(c => c.SetCellValue(ViewResources.LicenseeW_L), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnBaComm]).Verify(c => c.SetCellValue(ViewResources.BACommission), Times.Once);
            Mock.Get(firstHeaderRow.Object.Cells[ColumnAmtDueToBA]).Verify(c => c.SetCellValue(ViewResources.AmtDueToBA), Times.Once);

            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerWinLoss]).Verify(c => c.SetCellValue(ViewResources.WinLoss), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(ViewResources.Comm), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                 .With(x => x.DataList, new SortableList<CashOutWinLoss>())
                 .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void DrillDownLevelMain_WhenCall_HideColumnCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                 .With(x => x.DataList, new SortableList<CashOutWinLoss>())
                 .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.HideColumn(_sheet.Object, ColumnExternalId, ColumnSystemId), Times.Once);
        }

        [Theory]
        [InlineData((int)ReportLevel.Licensee)]
        [InlineData((int)ReportLevel.Customer)]
        public void DrillDownLevelNotMain_WhenCall_HideColumnCorrectly(int level)
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                 .With(x => x.DataList, new SortableList<CashOutWinLoss>())
                 .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, level).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.HideColumn(_sheet.Object, ColumnDate), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var bodyData = _fixture.Create<CashOutWinLoss>();
            var currencyData = _fixture.Build<CashOutWinLoss>()
                .With(x => x.Currency, 9).Create();
            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                 .With(x => x.DataList, new SortableList<CashOutWinLoss> { bodyData })
                 .With(x => x.TotalDataList, new SortableList<CashOutWinLoss> { currencyData })
                 .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[2];

            // Assert
            AssertCommonCellData(bodyRow, bodyData);
        }

        [Fact]
        public void AssignTotalData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var bodyData = _fixture.Create<CashOutWinLoss>();
            var currencyData = _fixture.Build<CashOutWinLoss>()
                .With(x => x.Currency, 9).Create();
            var totalData = _fixture.Create<CashOutWinLoss>();
            var data = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                 .With(x => x.DataList, new SortableList<CashOutWinLoss> { bodyData })
                 .With(x => x.TotalDataList, new SortableList<CashOutWinLoss> { currencyData })
                 .With(x => x.TotalData, totalData)
                 .Create();
            var metaModel = _fixture.Build<NewCashOutWinLossMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var totalRow = rows.Last();
            var totalRowIndex = RowContentTemplate + 2;

            // Assert
            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, totalRowIndex, totalRowIndex, ColumnIndex, ColumnCurrency), Times.Once);
            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(string.Format(ViewResources.Totalin, data.TotalData.CurrencyName)), Times.Once);
            AssertCommonCellData(totalRow, totalData);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var secondHeaderRow = rowList[1];
            var bodyRow = rowList[2];
            var totalRow = rowList[3];
            var totalSolidRow = rowList[4];
            var grandTotalRow = rowList[5];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotalCurrencyTemplate, RowTotalCurrencyTemplate))
                .Returns(totalRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotalCurrencyNoSolidBorder, RowTotalCurrencyNoSolidBorder))
               .Returns(totalSolidRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowGrantTotalTemplate, It.Is<int>(x => x >= RowContentTemplate)))
               .Returns(grandTotalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate + 1);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTotalCurrencyTemplate);
            var totalSolidRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTotalCurrencyNoSolidBorder);
            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowGrantTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, totalRow, totalSolidRow, grandTotalRow };
        }

        private NewCashOutWinLossExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new NewCashOutWinLossExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(NewCashOutWinLossMetaModel metaModel)
        {
            var displayedCurrency = string.IsNullOrEmpty(metaModel.BaseCurrency) ? metaModel.CurrencyName : metaModel.BaseCurrency.ToUpperInvariant();
            var currencyText = string.Format(ViewResources.Viewin, displayedCurrency);

            var fromDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownFromDate : metaModel.FromDate;
            var toDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownToDate : metaModel.ToDate;

            return $"{ViewResources.CashOutWinLoss}, {fromDate:MM/dd/yyyy} -> {toDate:MM/dd/yyyy}, {currencyText}";
        }

        private void AssertCommonCellData(Mock<IRow> row, CashOutWinLoss dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnDate]).Verify(c => c.SetCellValue(dataItem.DateString), Times.Once);
            Mock.Get(row.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(dataItem.ExternalId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(dataItem.SystemId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCurrency]).Verify(c => c.SetCellValue(dataItem.CurrencyName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnOver]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Turnover)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTotalTransaction]).Verify(c => c.SetCellValue(dataItem.TotalTransaction), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerWinLoss]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerTotal)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicenseeWL]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.LicenseeWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBaComm]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.BAComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnAmtDueToBA]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.AmtDueToBA)), Times.Once);
        }
    }
}