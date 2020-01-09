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
    public class PlatformWinLossMemberExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 1;
        private const int ColumnSystemId = 2;
        private const int ColumnCurrency = 3;
        private const int ColumnTurnover = 4;
        private const int ColumnTotalTrans = 5;
        private const int ColumnCustomerWl = 6;
        private const int ColumnCustomerCommission = 7;
        private const int ColumnCustomerTotal = 8;
        private const int ColumnLicenseeWl = 9;
        private const int ColumnBaCommission = 10;
        private const int ColumnAmtDueToBa = 11;
        private const int ColumnTemplateCount = 12;

        private const int RowNoInfoTemplate = 5;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;

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

            var data = _fixture.Build<SortableResultModel<PlatformWinLossMember>>()
                .With(x => x.DataList, new List<PlatformWinLossMember>())
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
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Currency,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.CustomerW_L,
                ViewResources.LicenseeW_L,
                ViewResources.BACommission,
                ViewResources.AmtDueToBA
            };

            var data = _fixture.Build<SortableResultModel<PlatformWinLossMember>>()
                .With(x => x.DataList, new List<PlatformWinLossMember>())
                .With(x => x.TotalRows, 0)
                .Create();

            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var firstHeaderRow = rows[0];
            var secondHeaderRow = rows[1];

            // Assert
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(ViewResources.WinLoss), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerCommission]).Verify(c => c.SetCellValue(ViewResources.Comm), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);

            ExcelTestHelper.AssertSetTextRowHeader(firstHeaderRow.Object, headerTextList);
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableResultModel<PlatformWinLossMember>>()
                .With(x => x.DataList, new List<PlatformWinLossMember>())
                .With(x => x.TotalRows, 0)
                .Create();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(_sheet.Object, _templateSheet.Object,
                RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void DoNotHasComm_WhenCall_HideColumnCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableResultModel<PlatformWinLossMember>>()
                .With(x => x.DataList, new List<PlatformWinLossMember>())
                .With(x => x.TotalRows, 0)
                .Create();
            var metaModel = _fixture.Build<PlatformWinLossMetaModel>()
                .With(x => x.HasComm, false)
                .Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.HideColumn(_sheet.Object, ColumnCustomerTotal, ColumnCustomerCommission), Times.Once);
            _excelBuilder.Verify(ex => ex.HideRow(_sheet.Object, RowHeaderTemplate + 1), Times.Once);
            _excelBuilder.Verify(ex => ex.HideColumn(_sheet.Object, ColumnBaCommission, ColumnAmtDueToBa), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var bodyData = _fixture.Build<PlatformWinLossMember>()
                .Create();
            var data = _fixture.Build<SortableResultModel<PlatformWinLossMember>>()
                .With(x => x.DataList, new List<PlatformWinLossMember> { bodyData })
                .With(x => x.TotalRows, 1)
                .Create();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[2];

            // Assert
            AssertRowData(bodyRow, bodyData);
        }

        [Fact]
        public void AssignTotalData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var totalItem = _fixture.Create<PlatformWinLossMember>();
            var bodyData = _fixture.Create<PlatformWinLossMember>();
            var data = _fixture.Build<SortableResultModel<PlatformWinLossMember>>()
                .With(x => x.DataList, new List<PlatformWinLossMember> { bodyData })
                .With(x => x.TotalRows, 1)
                .With(x => x.TotalData, totalItem)
                .Create();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var totalRow = rows.Last();
            var totalRowIndex = RowContentTemplate + 1;

            // Assert
            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, totalRowIndex, totalRowIndex, ColumnIndex, ColumnCurrency), Times.Once);
            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
            AssertCommonCellData(totalRow, totalItem);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var secondHeaderRow = rowList[1];
            var bodyRow = rowList[2];
            var totalRow = rowList[3];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotalTemplate, RowTotalTemplate))
                .Returns(totalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate + 1);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, totalRow };
        }

        private PlatformWinLossMemberExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new PlatformWinLossMemberExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(PlatformWinLossMetaModel metaModel)
        {
            var platformText = ViewResources.Platform + ": " + DataHelper.GetPlatformName(metaModel.PlatformType);
            var currencyText = metaModel.CurrencyId == 0
                ? ViewResources.AllCurrencies
                : $"{ViewResources.Currency} {metaModel.CurrencyName}";

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

        private void AssertRowData(Mock<IRow> row, PlatformWinLossMember dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(1), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCurrency]).Verify(c => c.SetCellValue(dataItem.CurrencyName), Times.Once);
            AssertCommonCellData(row, dataItem);
        }

        private void AssertCommonCellData(Mock<IRow> row, PlatformWinLossMember dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(dataItem.ExternalId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(dataItem.SystemId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnover]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Turnover)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTotalTrans]).Verify(c => c.SetCellValue(dataItem.TotalTransactions), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWinLoss)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerCommission]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerTotal)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicenseeWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.LicenseeWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBaCommission]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.BACommission)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnAmtDueToBa]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.AmtDueToBA)), Times.Once);
        }
    }
}