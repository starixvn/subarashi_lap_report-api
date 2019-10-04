using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Excels
{
    public class CustomerWinLossExcelTest : BaseReportExcelTest
    {
        private const string TemplateSheetName = "Template";

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;
        private const int RowNoInfoTemplate = 5;

        private const int ColumnIndex = 0;
        private const int ColumnExternalId = 1;
        private const int ColumnSystemId = 2;
        private const int ColumnTurnover = 3;
        private const int ColumnTotalTransaction = 4;
        private const int ColumnCustomerWl = 5;
        private const int ColumnCustomerComm = 6;
        private const int ColumnCustomerTotal = 7;
        private const int ColumnBetEffective = 8;
        private const int ColumnLicenseeWl = 9;
        private const int ColumnBaCommission = 10;
        private const int ColumnAmtDueToBa = 11;

        private const int ColumnTemplateCount = 12;

        private const string FormatDate = "MM/dd/yyyy";
        private const int ColumnCount = 12;
        private const string TemplateSheet = "TemplateSheet.xls";

        private static Fixture _fixture = new Fixture();

        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<IWorkbook> _workbook;

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForHeaderRows()
        {
            // Arrange
            var templateSheet = new Mock<ISheet>();
            var sheet = new Mock<ISheet>();
            var rows = CreateRows(sheet);
            var excel = CreateExcel(sheet, rows.ElementAt(0), rows.ElementAt(1), rows.ElementAt(2), rows.ElementAt(3), templateSheet);

            var data = _fixture.Build<ResultModel<CustomerWinLoss>>()
                .With(x => x.DataList, new List<CustomerWinLoss> { _fixture.Create<CustomerWinLoss>() })
                .With(x => x.TotalRows, 1)
                .Create();
            var metaModel = _fixture.Create<CustomerWinLossMetaModel>();

            var filterTitle = ExcelTitleBuilder.GetTitleCurrencyName(ViewResources.Viewin, metaModel.IsConvert, metaModel.BaseCurrency, metaModel.CurrencyName)
                            + ExcelTitleBuilder.GetLicenseeWLProduct(metaModel.ProductOptions, ViewResources.LicenseeW_L)
                            + (string.IsNullOrEmpty(metaModel.Username) ? string.Empty : $", {ViewResources.Username}: {metaModel.Username}");

            var reportTitle = string.Format(ViewResources.CustomerWinLossTitle,
                                        metaModel.FromDate.ToString(FormatDate),
                                        metaModel.ToDate.ToString(FormatDate),
                                        filterTitle);

            // Action
            excel.Export(TemplateSheet, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(sheet.Object, templateSheet.Object,
                RowTitleTemplate, RowTitleTemplate, ColumnTemplateCount, reportTitle), Times.Once);
        }

        [Fact]
        public void AssignHeader_WhenCall_AssignCorrectValueForTitleRow()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            var rows = CreateRows(sheet);
            var firstHeaderRow = rows.ElementAt(0);
            var secondHeaderRow = rows.ElementAt(1);
            var excel = CreateExcel(sheet, firstHeaderRow, secondHeaderRow, rows.ElementAt(2), rows.ElementAt(3));
            var headerTextList = new[]
            {
                "#", ViewResources.ExternalId, ViewResources.SystemId, ViewResources.Turnover,
                ViewResources.TotalTransaction, ViewResources.CustomerW_L, ViewResources.CustomerW_L,
                ViewResources.CustomerW_L, ViewResources.EffectiveBettingValue, ViewResources.LicenseeW_L,
                ViewResources.BACommission, ViewResources.AmtDueToBA
            };

            var data = _fixture.Build<ResultModel<CustomerWinLoss>>()
                .With(x => x.DataList, new List<CustomerWinLoss> { _fixture.Create<CustomerWinLoss>() })
                .With(x => x.TotalRows, 1)
                .Create();
            var metaModel = _fixture.Create<CustomerWinLossMetaModel>();

            // Action
            excel.Export(TemplateSheet, data, metaModel);

            // Assert
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(ViewResources.WinLoss), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(ViewResources.Comm), Times.Once);
            Mock.Get(secondHeaderRow.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);

            AssertSetTextRowHeader(firstHeaderRow.Object, headerTextList);
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var templateSheet = new Mock<ISheet>();
            var sheet = new Mock<ISheet>();
            var rows = CreateRows(sheet);
            var noInfoRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowNoInfoTemplate);
            var excel = CreateExcel(sheet, rows.ElementAt(0), rows.ElementAt(1), noInfoRow, rows.ElementAt(3), templateSheet);

            var data = _fixture.Build<ResultModel<CustomerWinLoss>>()
                .With(x => x.DataList, new List<CustomerWinLoss>())
                .With(x => x.TotalRows, 0)
                .Create();
            var metaModel = _fixture.Create<CustomerWinLossMetaModel>();

            // Action
            excel.Export(TemplateSheet, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.SetRowValue(sheet.Object, templateSheet.Object,
                RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void AssignSheetData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var templateSheet = new Mock<ISheet>();
            var sheet = new Mock<ISheet>();
            var rows = CreateRows(sheet);
            var bodyRow = rows.ElementAt(2);
            var excel = CreateExcel(sheet, rows.ElementAt(0), rows.ElementAt(1), bodyRow, rows.ElementAt(3), templateSheet);

            var dataItem = _fixture.Create<CustomerWinLoss>();
            var data = _fixture.Build<ResultModel<CustomerWinLoss>>()
                .With(x => x.DataList, new List<CustomerWinLoss> { dataItem })
                .With(x => x.TotalRows, 1)
                .Create();
            var metaModel = _fixture.Create<CustomerWinLossMetaModel>();

            // Action
            excel.Export(TemplateSheet, data, metaModel);

            // Assert
            Mock.Get(bodyRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(1), Times.Once);
            Mock.Get(bodyRow.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(dataItem.ExternalId), Times.Once);
            Mock.Get(bodyRow.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(dataItem.SystemId), Times.Once);
            AssertCommonCellData(bodyRow, dataItem);
        }

        [Fact]
        public void AssignSumData_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var templateSheet = new Mock<ISheet>();
            var sheet = new Mock<ISheet>();
            var rows = CreateRows(sheet);
            var totalRow = rows.ElementAt(3);
            var excel = CreateExcel(sheet, rows.ElementAt(0), rows.ElementAt(1), rows.ElementAt(2), totalRow, templateSheet);

            var totalItem = _fixture.Create<CustomerWinLoss>();
            var data = _fixture.Build<ResultModel<CustomerWinLoss>>()
                .With(x => x.DataList, new List<CustomerWinLoss> { _fixture.Create<CustomerWinLoss>() })
                .With(x => x.TotalRows, 1)
                .With(x => x.TotalData, totalItem)
                .Create();
            var metaModel = _fixture.Create<CustomerWinLossMetaModel>();
            var rowTotalIndex = checked(1 + RowContentTemplate);

            // Action
            excel.Export(TemplateSheet, data, metaModel);

            // Assert
            AssertCommonCellData(totalRow, totalItem);
            _excelBuilder.Verify(ex => ex.MergeCells(sheet.Object, rowTotalIndex, rowTotalIndex, ColumnIndex, ColumnSystemId), Times.Once);
            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);
        }

        private IEnumerable<Mock<IRow>> CreateRows(Mock<ISheet> sheet)
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowHeaderTemplate + 1);
            var bodyRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, totalRow };
        }

        private CustomerWinLossExcel CreateExcel(Mock<ISheet> sheet, Mock<IRow> firstHeaderRow, Mock<IRow> secondHeaderRow, Mock<IRow> bodyRow, Mock<IRow> totalRow, Mock<ISheet> template = null)
        {
            var templateSheet = template ?? new Mock<ISheet>();
            _excelBuilder = new Mock<IExcelBuilder>();
            _workbook = new Mock<IWorkbook>();
            _workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(templateSheet.Object);
            _excelBuilder.Setup(ex => ex.LoadTemplate(TemplateSheet)).Returns(_workbook.Object);
            _excelBuilder.Setup(ex => ex.CloneSheet(_workbook.Object, TemplateSheetName, It.Is<string>(x => x == "Data" || x == "Data1"))).Returns(sheet.Object);

            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowTotalTemplate, RowTotalTemplate))
                .Returns(totalRow.Object);

            return new CustomerWinLossExcel(_excelBuilder.Object);
        }

        private void AssertCommonCellData(Mock<IRow> row, CustomerWinLoss dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnTurnover]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Turnover)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTotalTransaction]).Verify(c => c.SetCellValue(dataItem.TotalTransaction), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.PlayerComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.CustomerWL + dataItem.PlayerComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBetEffective]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.BetEffective)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicenseeWl]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.LicenseeWL)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBaCommission]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.BAComm)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnAmtDueToBa]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.AmtDueToBA)), Times.Once);
        }
    }
}