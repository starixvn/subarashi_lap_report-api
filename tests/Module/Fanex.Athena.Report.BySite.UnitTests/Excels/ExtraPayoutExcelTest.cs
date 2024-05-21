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
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class ExtraPayoutExcelTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnProduct = 5;
        private const int ColumnPromotionType = 6;
        private const int ColumnTurnOver = 7;
        private const int ColumnTotalTransaction = 8;
        private const int ColumnCustomerAmount = 9;
        private const int ColumnLicSharingAmount = 10;

        private const int RowNoInfoTemplate = 8;
        private const int ColumnTemplateCount = 11;

        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowTotalCurrencyTemplate = 5;
        private const int RowGrantTotalTemplate = 7;

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

            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                .With(x => x.DataList, new SortableList<ExtraPayout>())
                .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
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

            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                .With(x => x.DataList, new SortableList<ExtraPayout>())
                .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();
            var headerTextList = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Currency,
                ViewResources.Product,
                ViewResources.PromotionType,
                ViewResources.Turnover,
                ViewResources.TotalTransaction,
                ViewResources.CustomerAmount,
                ViewResources.LicSharingAmount
            };

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

            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                 .With(x => x.DataList, new SortableList<ExtraPayout>())
                 .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
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

            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                 .With(x => x.DataList, new SortableList<ExtraPayout>())
                 .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            _excelBuilder.Verify(ex => ex.HideColumn(_sheet.Object, ColumnExternalId, ColumnSystemId, ColumnPromotionType), Times.Once);
        }

        [Fact]
        public void DrillDownLevelMember_WhenCall_HideColumnCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                 .With(x => x.DataList, new SortableList<ExtraPayout>())
                 .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
                .With(x => x.DrillDownLevel, 1).Create();

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

            var bodyData = _fixture.Create<ExtraPayout>();
            var currencyData = _fixture.Build<ExtraPayout>()
                .With(x => x.Currency, 9).Create();
            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                 .With(x => x.DataList, new SortableList<ExtraPayout> { bodyData })
                 .With(x => x.TotalDataList, new SortableList<ExtraPayout> { currencyData })
                 .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[2];

            // Assert
            AssertCommonCellData(bodyRow, bodyData);
        }

        [Fact]
        public void AssignGrandTotal_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var bodyData = _fixture.Create<ExtraPayout>();
            var currencyData = _fixture.Build<ExtraPayout>()
                .With(x => x.Currency, 9).Create();
            var totalData = _fixture.Create<ExtraPayout>();
            var data = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                 .With(x => x.DataList, new SortableList<ExtraPayout> { bodyData })
                 .With(x => x.TotalDataList, new SortableList<ExtraPayout> { currencyData })
                 .With(x => x.TotalData, totalData)
                 .Create();
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>()
                .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var totalRow = rows.Last();
            var totalRowIndex = RowContentTemplate + 2;

            // Assert
            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, totalRowIndex, totalRowIndex, ColumnIndex, ColumnPromotionType), Times.Once);
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
            var grandTotalRow = rowList[4];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate + 1, RowHeaderTemplate + 1))
                .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotalCurrencyTemplate, It.Is<int>(x => x >= RowContentTemplate)))
                .Returns(totalRow.Object);
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
            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowGrantTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, totalRow, grandTotalRow };
        }

        private ExtraPayoutExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName)).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new ExtraPayoutExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(ExtraPayoutMetaModel metaModel)
        {
            var displayedCurrency = string.IsNullOrEmpty(metaModel.BaseCurrency) ? metaModel.CurrencyName : metaModel.BaseCurrency.ToUpperInvariant();
            var currencyText = $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            var baseCurrencyText = metaModel.IsConvertAllToBaseCurrency ? $", {ViewResources.BaseCurrency}: {displayedCurrency}" : "";
            var productText = $"{ViewResources.LicenseeW_L}: ({metaModel.ProductCodeList.Replace(",", ", ")})";
            var userNameText = string.IsNullOrEmpty(metaModel.Username) ? "" : $", {ViewResources.Username}: {metaModel.Username}";

            var fromDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownFromDate : metaModel.FromDate;
            var toDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ? metaModel.DrillDownToDate : metaModel.ToDate;

            return $"{ViewResources.ExtraPayout}, {fromDate:MM/dd/yyyy} -> {toDate:MM/dd/yyyy}, {currencyText}{baseCurrencyText}, {productText}{userNameText}";
        }

        private void AssertCommonCellData(Mock<IRow> row, ExtraPayout dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnDate]).Verify(c => c.SetCellValue(dataItem.DateString), Times.Once);
            Mock.Get(row.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(dataItem.Nickname), Times.Once);
            Mock.Get(row.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(dataItem.Username), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCurrency]).Verify(c => c.SetCellValue(dataItem.CurrencyName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnProduct]).Verify(c => c.SetCellValue(dataItem.ProductName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnPromotionType]).Verify(c => c.SetCellValue(dataItem.PromotionName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnOver]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Turnover)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTotalTransaction]).Verify(c => c.SetCellValue(dataItem.BetCount), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerAmount]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Amount)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicSharingAmount]).Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.SharingAmount)), Times.Once);
        }
    }
}