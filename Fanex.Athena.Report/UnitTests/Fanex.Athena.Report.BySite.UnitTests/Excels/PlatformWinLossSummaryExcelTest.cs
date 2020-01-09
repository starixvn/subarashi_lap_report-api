using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Helpers;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class PlatformWinLossSummaryExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";
        private const int ColumnPlatform = 0;
        private const int ColumnTurnover = 1;
        private const int ColumnTurnoverPercent = 2;
        private const int ColumnTotalTransaction = 3;
        private const int ColumnCustomerWinLoss = 4;
        private const int ColumnCustomerComm = 5;
        private const int ColumnCustomerTotal = 6;
        private const int ColumnLicenseeWinLoss = 7;

        private const int ColumnNoContent = 8;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowNoInfoTemplate = 6;
        private const int RowTotal = 5;

        private const int RowParentContentTemplate = 3;
        private const int RowChildContentTemplate = 4;

        private readonly IEnumerable<ProductOption> ProductOptions = new List<ProductOption>
                {
                    new ProductOption { Code = "SB", Status = true },
                    new ProductOption { Code = "NG", Status = false }
                };

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

            var data = _fixture.Build<ResultModel<PlatformWinLossSummary>>()
                .With(x => x.DataList, new List<PlatformWinLossSummary>())
                .With(x => x.TotalRows, 1)
                .Create();
            var metaModel = _fixture.Build<PlatformWinLossMetaModel>()
                .With(x => x.ProductOptions, ProductOptions)
                .Create();

            var reportTitle = CreateTitle(metaModel);

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowTitleTemplate, RowTitleTemplate, ColumnNoContent, reportTitle), Times.Once);
        }

        [Fact]
        public void AssignHeader_WhenCall_AssignCorrectValueForHeaderRows()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();
            var headerTextList = new[]
            {
                ViewResources.Platform,
                ViewResources.Turnover,
                ViewResources.TurnoverPercent,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L,
            };

            var data = _fixture.Build<ResultModel<PlatformWinLossSummary>>()
                .With(x => x.DataList, new List<PlatformWinLossSummary>())
                .With(x => x.TotalRows, 0)
                .Create();

            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var firstHeaderRow = rows[0];
            var secondHeaderRow = rows[1];

            // Assert
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerWinLoss]).Verify(c => c.SetCellValue(ViewResources.WinLoss), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(ViewResources.Comm), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);

            ExcelTestHelper.AssertSetTextRowHeader(firstHeaderRow.Object, headerTextList);
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<ResultModel<PlatformWinLossSummary>>()
                .With(x => x.DataList, new List<PlatformWinLossSummary>())
                .With(x => x.TotalRows, 0)
                .Create();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowNoInfoTemplate, RowParentContentTemplate, ColumnNoContent, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void DoNotHasComm_WhenCall_HideColumnCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<ResultModel<PlatformWinLossSummary>>()
                .With(x => x.DataList, new List<PlatformWinLossSummary>())
                .With(x => x.TotalRows, 0)
                .Create();
            var metaModel = _fixture.Build<PlatformWinLossMetaModel>()
                .With(x => x.HasComm, false)
                .Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.HideColumn(_sheet.Object, ColumnCustomerTotal, ColumnCustomerComm), Times.Once);
            _excelBuilder.Verify(ex => ex.HideRow(_sheet.Object, RowHeaderTemplate + 1), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var parentData = _fixture.Build<PlatformWinLossSummary>()
                .With(x => x.Level, 0)
                .Create();
            var childData = _fixture.Build<PlatformWinLossSummary>()
                .With(x => x.Level, 1)
                .Create();
            var data = _fixture.Build<ResultModel<PlatformWinLossSummary>>()
                .With(x => x.DataList, new List<PlatformWinLossSummary> { parentData, childData })
                .With(x => x.TotalRows, 1)
                .Create();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var parentBodyRow = rows[2];
            var childBodyRow = rows[3];

            // Assert
            AssertRowData(parentBodyRow, parentData);
            AssertRowData(childBodyRow, childData);
        }

        [Fact]
        public void AssignTotalData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var totalItem = _fixture.Create<PlatformWinLossSummary>();
            var parentData = _fixture.Build<PlatformWinLossSummary>()
                .With(x => x.Level, 0)
                .Create();
            var childData = _fixture.Build<PlatformWinLossSummary>()
                .With(x => x.Level, 1)
                .Create();
            var data = _fixture.Build<ResultModel<PlatformWinLossSummary>>()
                .With(x => x.DataList, new List<PlatformWinLossSummary> { parentData, childData })
                .With(x => x.TotalRows, 1)
                .With(x => x.TotalData, totalItem)
                .Create();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var totalRow = rows.Last();

            // Assert
            Mock.Get(totalRow.Object.Cells[ColumnPlatform]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
            AssertCommonCellData(totalRow, totalItem);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var secondHeaderRow = rowList[1];
            var bodyParentRow = rowList[2];
            var bodyChildRow = rowList[3];
            var totalRow = rowList[4];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowParentContentTemplate || x == RowNoInfoTemplate), RowParentContentTemplate))
                .Returns(bodyParentRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowChildContentTemplate, RowChildContentTemplate))
                .Returns(bodyChildRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotal, RowTotal))
                .Returns(totalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnNoContent, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnNoContent, RowHeaderTemplate + 1);
            var bodyParentRow = ExcelTestHelper.MockGetRow(_sheet, ColumnNoContent, RowParentContentTemplate);
            var bodyChildRow = ExcelTestHelper.MockGetRow(_sheet, ColumnNoContent, RowChildContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnNoContent, RowTotal);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyParentRow, bodyChildRow, totalRow };
        }

        private PlatformWinLossSummaryExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new PlatformWinLossSummaryExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(PlatformWinLossMetaModel metaModel)
        {
            var platformText = ViewResources.Platform + ": " + DataHelper.GetPlatformName(metaModel.PlatformType);
            var currencyText = metaModel.CurrencyId == 0 ?
                ViewResources.AllCurrencies :
                string.Format("{0} {1}", ViewResources.Currency, metaModel.CurrencyName);

            var baseCurrencyText = string.Format(
                ViewResources.Viewin,
                !string.IsNullOrWhiteSpace(metaModel.BaseCurrency) ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName);

            var productTitle = $"{ViewResources.Product}: SB";
            return string.Format(
                ViewResources.PlatformWinLossTitle,
                metaModel.FromDate.ToString("MM/dd/yyyy"),
                metaModel.ToDate.ToString("MM/dd/yyyy"),
                currencyText,
                baseCurrencyText,
                platformText) + ", " + productTitle;
        }

        private void AssertRowData(Mock<IRow> row, PlatformWinLossSummary dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnPlatform]).Verify(c => c.SetCellValue(dataItem.Name), Times.Once);
            AssertCommonCellData(row, dataItem);
        }

        private void AssertCommonCellData(Mock<IRow> row, PlatformWinLossSummary dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnTurnover]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Turnover)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnoverPercent]).Verify(c => c.SetCellValue(string.Format("{0:0.00}%", dataItem.TurnoverPercent)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTotalTransaction]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.TotalTransactions)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerWinLoss]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWinLoss)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerTotal)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicenseeWinLoss]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.LicenseeWL)), Times.Once);
        }
    }
}