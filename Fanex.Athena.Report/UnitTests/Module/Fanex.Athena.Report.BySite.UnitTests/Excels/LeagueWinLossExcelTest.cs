using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class LeagueWinLossExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

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

        private const string TemplatePath = "TemplatePath";
        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<ISheet> _sheet;
        private Mock<ISheet> _templateSheet;

        public LeagueWinLossExcelTest()
        {
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                .With(x => x.DataList, new SortableList<LeagueWinLoss>())
                .Create();
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().Create();

            var reportTitle = CreateTitle(metaModel, data);

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

            var data = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                .With(x => x.DataList, new SortableList<LeagueWinLoss>())
                .Create();
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().Create();

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

            var data = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>().Create();
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().Create();

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

            var detailData = _fixture.Create<LeagueWinLoss>();
            var bodyData = new SortableList<LeagueWinLoss> {
                _fixture.Build<LeagueWinLoss>()
                .With(x => x.Detail, new List<LeagueWinLoss>{ detailData })
                .Create()};
            var data = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                .With(x => x.DataList, bodyData)
                .Create();
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[1];

            // Assert
            Mock.Get(bodyRow.Object.Cells[ColumnSport]).Verify(c => c.SetCellValue(data.DataList[0].SportName), Times.Once);
            AssertCommonCellData(bodyRow, data.DataList[0]);
            Mock.Get(bodyRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(1), Times.Once);
            AssertCommonCellData(bodyRow, detailData);
        }

        [Fact]
        public void AssignGrandTotalData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var detailData = _fixture.Create<LeagueWinLoss>();
            var bodyData = new SortableList<LeagueWinLoss> {
                _fixture.Build<LeagueWinLoss>()
                .With(x => x.Detail, new List<LeagueWinLoss>{ detailData })
                .Create()};
            var data = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                .With(x => x.DataList, bodyData)
                .Create();
            var metaModel = _fixture.Build<LeagueWinLossMetaModel>().Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var grandTotalRow = rows.Last();
            var totalRowIndex = RowContentTemplate + 1;

            // Assert
            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, totalRowIndex, totalRowIndex, ColumnIndex, 4), Times.Once);
            Mock.Get(grandTotalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
            AssertCommonCellData(grandTotalRow, data.TotalData);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var bodyRow = rowList[1];
            var totalRow = rowList[3];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowGrantTotalTemplate, It.Is<int>(x => x >= RowContentTemplate)))
                .Returns(totalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate + 1);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, ColumnTemplateCount);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, totalRow };
        }

        private LeagueWinLossExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new LeagueWinLossExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(LeagueWinLossMetaModel metaModel, SortableReportResultModel<LeagueWinLoss> data)
        {
            var currencyText = $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            var baseCurrency = !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName;
            var baseCurrencyText = $"{ViewResources.BaseCurrency}: {baseCurrency}";
            var extendInfo = "";
            var userNameText = "";
            if (!string.IsNullOrWhiteSpace(metaModel.UserName))
            {
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

            return string.Format(
                ViewResources.LeagueWinLossTitle,
                metaModel.FromDate.ToString("MM/dd/yyyy"),
                metaModel.ToDate.ToString("MM/dd/yyyy"),
                currencyText,
                baseCurrencyText) + userNameText + (string.IsNullOrEmpty(extendInfo) ? "" : extendInfo);
        }

        private void AssertCommonCellData(Mock<IRow> row, LeagueWinLoss dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnLeague]).Verify(c => c.SetCellValue(dataItem.LeagueName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnMatch]).Verify(c => c.SetCellValue(dataItem.MatchName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBetType]).Verify(c => c.SetCellValue(dataItem.BetTypeName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBetCount]).Verify(c => c.SetCellValue(dataItem.BetCount), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnover]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.TurnOver)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicenseeWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.LicenseeWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBAComm]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.BAComm)), Times.Once);
        }
    }
}