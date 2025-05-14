using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using Moq;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class PlayerSabaCoinsExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";
        private const string TemplatePath = "TemplatePath";

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 1;
        private const int ColumnSystemId = 2;
        private const int ColumnOpeningBalance = 3;
        private const int ColumnReward_Promotion = 4;
        private const int ColumnExchangeToCash = 5;
        private const int ColumnTurnOver = 6;
        private const int ColumnPendingFunds = 7;
        private const int ColumnPlayerWL = 8;
        private const int ColumnClosingBalance = 9;

        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 10;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowGrantTotalTemplate = 5;

        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<ISheet> _sheet;
        private Mock<ISheet> _templateSheet;

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>().Create();
            var title = CreateTitle(metaModel);
            var data = _fixture.Build<SortableReportResultModel<PlayerSabaCoins>>()
                .With(x => x.DataList, new SortableList<PlayerSabaCoins>())
                .Create();

            // Act
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(
                ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object, RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, title),
                Times.Once);
        }

        [Fact]
        public void AssignHeader_WhenCall_AssignCorrectValueForHeaderRows()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>().Create();
            var data = _fixture.Build<SortableReportResultModel<PlayerSabaCoins>>()
                .With(x => x.DataList, new SortableList<PlayerSabaCoins>())
                .Create();
            var headerTextList = new[]
                        {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.OpeningBalance,
                ViewResources.Reward_Promotion,
                ViewResources.ExchangeToCash,
                ViewResources.Turnover,
                ViewResources.PendingFunds,
                ViewResources.PlayerW_L,
                ViewResources.ClosingBalance
            };

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var firstHeaderRow = rows[1];

            // Assert
            ExcelTestHelper.AssertSetTextRowHeader(firstHeaderRow.Object, headerTextList);
        }

        [Fact]
        public void AssignNoInfoRow_WhenDataIsEmpty_AssignsNoInfoRow()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>().Create();
            var data = _fixture.Build<SortableReportResultModel<PlayerSabaCoins>>()
                .With(x => x.DataList, new SortableList<PlayerSabaCoins>())
                .Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenDataExists_SetsDataCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>().Create();
            var account = _fixture.Create<PlayerSabaCoinsAccount>();
            var item = _fixture.Build<PlayerSabaCoins>()
                .With(x => x.Accounts, new List<PlayerSabaCoinsAccount> { account })
                .Create();
            var data = _fixture.Build<SortableReportResultModel<PlayerSabaCoins>>()
                .With(x => x.DataList, new SortableList<PlayerSabaCoins> { item })
                .Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[2];

            // Assert
            AssertCellUserData(bodyRow, item);
            AssertCommonCellData(bodyRow, item);
        }

        [Fact]
        public void AssignGrandTotal_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();
            var metaModel = _fixture.Build<PlayerSabaCoinsMetaModel>().Create();
            var account = _fixture.Create<PlayerSabaCoinsAccount>();
            var item = _fixture.Build<PlayerSabaCoins>()
                .With(x => x.Accounts, new List<PlayerSabaCoinsAccount> { account })
                .Create();
            var totalData = _fixture.Create<PlayerSabaCoins>();
            var data = _fixture.Build<SortableReportResultModel<PlayerSabaCoins>>()
                .With(x => x.DataList, new SortableList<PlayerSabaCoins> { item })
                .With(x => x.TotalData, totalData)
                .Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var totalRow = rows.Last();
            var totalRowIndex = RowContentTemplate + 1;

            // Assert
            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, totalRowIndex, totalRowIndex, ColumnIndex, ColumnSystemId), Times.Once);
            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
            AssertCommonCellData(totalRow, totalData);
        }

        private PlayerSabaCoinsExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(It.IsAny<string>())).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new PlayerSabaCoinsExcel(_excelBuilder.Object);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var secondHeaderRow = rowList[1];
            var bodyRow = rowList[2];
            var grandTotalRow = rowList[3];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTitleTemplate, RowTitleTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), It.Is<int>(x => x >= RowContentTemplate)))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowGrantTotalTemplate, It.Is<int>(x => x >= RowContentTemplate)))
               .Returns(grandTotalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTitleTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowGrantTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, grandTotalRow };
        }

        private string CreateTitle(PlayerSabaCoinsMetaModel metaModel)
        {
            return $"{ViewResources.PlayerSabaCoins}, {metaModel.FromDate:MM/dd/yyyy} -> {metaModel.ToDate:MM/dd/yyyy}";
        }

        private void AssertCommonCellData(Mock<IRow> row, PlayerSabaCoins item)
        {
            Mock.Get(row.Object.Cells[ColumnOpeningBalance]).Verify(c => c.SetCellValue((double)item.OpeningBalance), Times.Once);
            Mock.Get(row.Object.Cells[ColumnReward_Promotion]).Verify(c => c.SetCellValue((double)item.RewardPromotionAmt), Times.Once);
            Mock.Get(row.Object.Cells[ColumnExchangeToCash]).Verify(c => c.SetCellValue((double)item.ExchangeAmt), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnOver]).Verify(c => c.SetCellValue((double)item.Turnover), Times.Once);
            Mock.Get(row.Object.Cells[ColumnPendingFunds]).Verify(c => c.SetCellValue((double)item.Outstanding), Times.Once);
            Mock.Get(row.Object.Cells[ColumnPlayerWL]).Verify(c => c.SetCellValue((double)item.WinLoss), Times.Once);
            Mock.Get(row.Object.Cells[ColumnClosingBalance]).Verify(c => c.SetCellValue((double)item.ClosingBalance), Times.Once);
        }

        private void AssertCellUserData(Mock<IRow> row, PlayerSabaCoins item)
        {
            Mock.Get(row.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(1), Times.Once);
            Mock.Get(row.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(item.Accounts.ElementAt(0).ExternalId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(item.Accounts.ElementAt(0).Username), Times.Once);
        }
    }
}