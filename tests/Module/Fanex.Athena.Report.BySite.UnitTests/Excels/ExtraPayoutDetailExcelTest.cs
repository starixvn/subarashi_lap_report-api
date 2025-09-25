namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using BySite.DTOs;
    using BySite.Excels;
    using BySite.MetaModels;
    using Common.Excel.Interfaces;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using Moq;
    using NPOI.SS.UserModel;
    using Resources;
    using Resources.Constants;
    using TestHelpers;
    using Xunit;

    public class ExtraPayoutDetailExcelTest
    {
        private const string TemplateSheetName = "Template";

        private const string SheetName = "Data";

        private const int ColumnIndex = 0;

        private const int ColumnDate = 1;

        private const int ColumnRefNo = 2;

        private const int ColumnBetId = 3;

        private const int ColumnProduct = 4;

        private const int ColumnPromotionType = 5;

        private const int ColumnTurnOver = 6;

        private const int ColumnCustomerAmount = 7;

        private const int ColumnLicSharingAmount = 8;

        private const int RowNoInfoTemplate = 6;

        private const int ColumnTemplateCount = 9;

        private const int RowHeaderTemplate = 1;

        private const int RowTitleTemplate = 0;

        private const int RowContentTemplate = 3;

        private const int RowGrantTotalTemplate = 5;

        private const string TemplatePath = "TemplatePath";

        private readonly IFixture _fixture = new Fixture();

        private Mock<IExcelBuilder> _excelBuilder;

        private Mock<ISheet> _sheet;

        private Mock<ISheet> _templateSheet;

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                               .With(x => x.DataList, new SortableList<ExtraPayoutDetail>())
                               .Create();
            var metaModel = _fixture.Build<ExtraPayoutDetailMetaModel>()
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

            var data = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                               .With(x => x.DataList, new SortableList<ExtraPayoutDetail>())
                               .Create();
            var metaModel = _fixture.Build<ExtraPayoutDetailMetaModel>()
                                    .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();
            var headerTextList = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.RefNo,
                ViewResources.BetID,
                ViewResources.Product,
                ViewResources.PromotionType,
                ViewResources.Turnover,
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

            var data = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                               .With(x => x.DataList, new SortableList<ExtraPayoutDetail>())
                               .Create();
            var metaModel = _fixture.Build<ExtraPayoutDetailMetaModel>()
                                    .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

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

            var bodyData = _fixture.Create<ExtraPayoutDetail>();
            var currencyData = _fixture.Build<ExtraPayoutDetail>()
                                       .With(x => x.Currency, 9).Create();
            var data = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                               .With(x => x.DataList, new SortableList<ExtraPayoutDetail> { bodyData, })
                               .With(x => x.TotalDataList, new SortableList<ExtraPayoutDetail> { currencyData, })
                               .Create();
            var metaModel = _fixture.Build<ExtraPayoutDetailMetaModel>()
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

            var bodyData = _fixture.Create<ExtraPayoutDetail>();
            var currencyData = _fixture.Build<ExtraPayoutDetail>()
                                       .With(x => x.Currency, 9).Create();
            var totalData = _fixture.Create<ExtraPayoutDetail>();
            var data = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                               .With(x => x.DataList, new SortableList<ExtraPayoutDetail> { bodyData, })
                               .With(x => x.TotalDataList, new SortableList<ExtraPayoutDetail> { currencyData, })
                               .With(x => x.TotalData, totalData)
                               .Create();
            var metaModel = _fixture.Build<ExtraPayoutDetailMetaModel>()
                                    .With(x => x.DrillDownLevel, (int)ReportLevel.Main).Create();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var totalRow = rows.Last();

            // Assert
            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(string.Format(ViewResources.Totalin, metaModel.BaseCurrency.ToUpperInvariant())), Times.Once);
            AssertCommonCellData(totalRow, totalData);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var firstHeaderRow = rowList[0];
            var secondHeaderRow = rowList[1];
            var bodyRow = rowList[2];
            var grandTotalRow = rowList[3];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate,
                              RowHeaderTemplate))
                         .Returns(firstHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate + 1,
                              RowHeaderTemplate + 1))
                         .Returns(secondHeaderRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object,
                              It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                         .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowGrantTotalTemplate,
                              It.Is<int>(x => x >= RowContentTemplate)))
                         .Returns(grandTotalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var firstHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var secondHeaderRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate + 1);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowGrantTotalTemplate);

            return new List<Mock<IRow>> { firstHeaderRow, secondHeaderRow, bodyRow, grandTotalRow, };
        }

        private ExtraPayoutDetailExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, TemplateSheetName, SheetName))
                         .Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new ExtraPayoutDetailExcel(_excelBuilder.Object);
        }

        private static string CreateTitle(ExtraPayoutDetailMetaModel metaModel)
        {
            var currencyText = string.Format(ViewResources.Viewin, metaModel.CurrencyName.ToUpperInvariant());
            var usernameText = $"- {metaModel.ExternalId} ({metaModel.SystemId})";

            var fromDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ?
                metaModel.DrillDownFromDate :
                metaModel.FromDate;
            var toDate = metaModel.DrillDownLevel != (int)ReportLevel.Main ?
                metaModel.DrillDownToDate :
                metaModel.ToDate;

            return
                $"{ViewResources.PromotionDetails}, {fromDate.ToString("MM/dd/yyyy")} -> {toDate.ToString("MM/dd/yyyy")} {usernameText} - {currencyText}";
        }

        private void AssertCommonCellData(Mock<IRow> row, ExtraPayoutDetail dataItem)
        {
            Mock.Get(row.Object.Cells[ColumnDate]).Verify(c => c.SetCellValue(dataItem.DateString), Times.Once);
            Mock.Get(row.Object.Cells[ColumnRefNo]).Verify(c => c.SetCellValue(dataItem.Refno), Times.Once);
            Mock.Get(row.Object.Cells[ColumnBetId]).Verify(c => c.SetCellValue(dataItem.BetID), Times.Once);
            Mock.Get(row.Object.Cells[ColumnProduct]).Verify(c => c.SetCellValue(dataItem.ProductName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnPromotionType])
                .Verify(c => c.SetCellValue(dataItem.PromotionName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnTurnOver])
                .Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Turnover)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCustomerAmount])
                .Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.Amount)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnLicSharingAmount])
                .Verify(c => c.SetCellValue(Convert.ToDouble(dataItem.SharingAmount)), Times.Once);
        }
    }
}