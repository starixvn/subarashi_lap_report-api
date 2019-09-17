using System.Collections.Generic;
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

        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 3;
        private const int RowTotalTemplate = 4;

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

        private const string FormatDate = "MM/dd/yyyy";
        private const int ColumnCount = 12;
        private const string TemplateSheet = "TemplateSheet.xls";

        private static Fixture _fixture = new Fixture();
        private static readonly CustomerWinLoss DataItem = _fixture.Create<CustomerWinLoss>();

        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<IWorkbook> _workbook;

        [Fact]
        public void AssignHeader_WhenCall_CallRelevantMethods()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            var row1 = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowHeaderTemplate);
            var row2 = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowHeaderTemplate + 1);
            var excel = CreateExcel(sheet, row1, row2);
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
            Mock.Get(row2.Object.Cells[ColumnCustomerWl]).Verify(c => c.SetCellValue(ViewResources.WinLoss), Times.Once);
            Mock.Get(row2.Object.Cells[ColumnCustomerComm]).Verify(c => c.SetCellValue(ViewResources.Comm), Times.Once);
            Mock.Get(row2.Object.Cells[ColumnCustomerTotal]).Verify(c => c.SetCellValue(ViewResources.Total), Times.Once);

            AssertSetTextRowHeader(row1.Object, headerTextList);
        }

        private CustomerWinLossExcel CreateExcel(Mock<ISheet> sheet, Mock<IRow> firstRow = null, Mock<IRow> secondRow = null)
        {
            var templateSheet = new Mock<ISheet>();
            _excelBuilder = new Mock<IExcelBuilder>();
            _workbook = new Mock<IWorkbook>();
            _workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(templateSheet.Object);
            _excelBuilder.Setup(ex => ex.LoadTemplate(TemplateSheet)).Returns(_workbook.Object);
            _excelBuilder.Setup(ex => ex.CloneSheet(_workbook.Object, TemplateSheetName, "Data1")).Returns(sheet.Object);

            var row1 = firstRow ?? new Mock<IRow>();
            var row2 = secondRow ?? new Mock<IRow>();
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(row1.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(row2.Object);

            var bodyRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowContentTemplate);
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowContentTemplate, RowContentTemplate))
                .Returns(bodyRow.Object);

            var totalRow = ExcelTestHelper.MockGetRow(sheet, ColumnCount, RowTotalTemplate);
            _excelBuilder.Setup(ex => ex.CopyRow(sheet.Object, templateSheet.Object, RowTotalTemplate, RowTotalTemplate))
                .Returns(totalRow.Object);

            return new CustomerWinLossExcel(_excelBuilder.Object);
        }
    }
}