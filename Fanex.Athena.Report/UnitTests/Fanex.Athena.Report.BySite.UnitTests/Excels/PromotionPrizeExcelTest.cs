using System;
using System.Collections.Generic;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class PromotionPrizeExcelTest
    {
        private readonly IFixture _fixture = new Fixture();
        private const string TemplateSheetName = "Template";
        private const string DefaultSheetName = "Data";
        public const string FormatDate = "MM/dd/yyyy";

        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnPromotionType = 2;
        private const int ColumnCurrency = 3;
        private const int ColumnExternalId = 4;
        private const int ColumnSystemId = 5;
        private const int ColumnAmount = 6;

        private const int ColumnTemplateCount = 7;

        private const int RowTitleTemplate = 0;
        private const int RowHeaderTemplate = 1;
        private const int RowContentTemplate = 2;
        private const int RowTotal = 4;
        private const int RowGrandTotal = 6;
        private const int RowNoInfoTemplate = 7;

        private const string TemplatePath = "TemplatePath";
        private Mock<IExcelBuilder> _excelBuilder;
        private Mock<ISheet> _sheet;
        private Mock<ISheet> _templateSheet;

        private const string ProductCode = "SB";
        private readonly string ProductName = ViewResources.ResourceManager.GetString(ProductCode);
        private readonly string SabaPromotion = ViewResources.ResourceManager.GetString(Product.SPR);

        [Fact]
        public void AssignTitle_WhenCall_AssignCorrectValueForTitleRows()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = new Dictionary<string, PromotionPrizeResultModel>();
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

            var currencyTitle = metaModel.CurrencyId <= 0 ? ViewResources.AllCurrencies : $"{ViewResources.Currency}: {metaModel.CurrencyName}";
            var reportTitle = $"{ViewResources.PromotionPrize}, {metaModel.FromDate.ToString(FormatDate)} --> {metaModel.ToDate.ToString(FormatDate)} - {currencyTitle}";

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
                ViewResources.PromotionType,
                ViewResources.Currency,
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.Amount
            };

            var data = new Dictionary<string, PromotionPrizeResultModel>();
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var headerRow = rows[0];

            // Assert
            ExcelTestHelper.AssertSetTextRowHeader(headerRow.Object, headerTextList);
        }

        [Fact]
        public void AssignNoInfoRow_WhenCall_AssignCorrectValueForContent()
        {
            // Arrange
            var excel = CreateExcel();
            CreateRows();

            var data = new Dictionary<string, PromotionPrizeResultModel>();
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

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

            var bodyData = _fixture.Build<PromotionPrizeResultModel>()
                .With(x => x.Data, new List<PromotionPrize> { _fixture.Create<PromotionPrize>() })
                .With(x => x.SummaryTotal, new List<PromotionPrize> { _fixture.Create<PromotionPrize>() })
                .Create();
            var data = new Dictionary<string, PromotionPrizeResultModel>
            {
                { ProductCode, bodyData }
            };
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

            var totalRow = rows[2];
            _sheet.Setup(s => s.GetRow(RowContentTemplate + 1)).Returns(totalRow.Object);

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var bodyRow = rows[1];

            // Assert
            AssertRowData(bodyRow, bodyData);
        }

        [Fact]
        public void AssignSummaryTotal_WhenCall_CallSetValueCorrectly()
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var totalData = _fixture.Create<PromotionPrize>();
            var bodyData = _fixture.Build<PromotionPrizeResultModel>()
                .With(x => x.Data, new List<PromotionPrize> { _fixture.Create<PromotionPrize>() })
                .With(x => x.SummaryTotal, new List<PromotionPrize> { totalData })
                .Create();
            var data = new Dictionary<string, PromotionPrizeResultModel>
            {
                { ProductCode, bodyData }
            };
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

            var totalRow = rows[2];
            _sheet.Setup(s => s.GetRow(RowContentTemplate + 1)).Returns(totalRow.Object);

            // Action
            excel.Export(TemplatePath, data, metaModel);

            // Assert
            Mock.Get(totalRow.Object.Cells[ColumnAmount]).Verify(c => c.SetCellValue(Convert.ToDouble(totalData.Amount)), Times.Once);
            Mock.Get(totalRow.Object.Cells[ColumnPromotionType]).Verify(c => c.SetCellValue(totalData.PromotionType), Times.Once);

            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, RowContentTemplate + 1, RowContentTemplate + 1, ColumnPromotionType, ColumnSystemId), Times.Once);

            Mock.Get(totalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(string.Format(ViewResources.Totalin, metaModel.DefaultBaseCurrency)), Times.Once);
            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, RowContentTemplate + 1, RowContentTemplate + 1, ColumnIndex, ColumnDate), Times.Once);
        }

        [Theory]
        [InlineData(ProductCode, 2)]
        [InlineData(Product.SPR, 1)]
        public void AssignGrandTotal_WhenCall_CallSetValueCorrectly(string productCode, int rowIndexCount)
        {
            // Arrange
            var excel = CreateExcel();
            var rows = CreateRows();

            var grandTotalData = _fixture.Create<decimal>();
            var bodyData = _fixture.Build<PromotionPrizeResultModel>()
                .With(x => x.Data, new List<PromotionPrize> { _fixture.Create<PromotionPrize>() })
                .With(x => x.SummaryTotal, new List<PromotionPrize> { _fixture.Create<PromotionPrize>() })
                .With(x => x.GrandTotal, grandTotalData)
                .Create();
            var data = new Dictionary<string, PromotionPrizeResultModel>
            {
                { productCode, bodyData }
            };
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

            var totalRow = rows[2];
            var grandTotalRow = rows[3];
            _sheet.Setup(s => s.GetRow(RowContentTemplate + 1)).Returns(totalRow.Object);

            // Action
            excel.Export(TemplatePath, data, metaModel);
            var rowIndex = RowContentTemplate + rowIndexCount;

            // Assert
            Mock.Get(grandTotalRow.Object.Cells[ColumnAmount]).Verify(c => c.SetCellValue(Convert.ToDouble(grandTotalData)), Times.Once);
            Mock.Get(grandTotalRow.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(string.Format(ViewResources.Totalin, metaModel.DefaultBaseCurrency)), Times.Once);

            _excelBuilder.Verify(x => x.MergeCells(_sheet.Object, rowIndex, rowIndex, ColumnIndex, ColumnSystemId), Times.Once);
        }

        private IList<Mock<IRow>> CreateRows()
        {
            var rowList = CreateRowList();
            var headerRow = rowList[0];
            var bodyRow = rowList[1];
            var totalRow = rowList[2];
            var grandTotalRow = rowList[3];

            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(headerRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, It.Is<int>(x => x == RowContentTemplate || x == RowNoInfoTemplate), RowContentTemplate))
                .Returns(bodyRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowTotal, RowContentTemplate + 1))
                .Returns(totalRow.Object);
            _excelBuilder.Setup(ex => ex.CopyRow(_sheet.Object, _templateSheet.Object, RowGrandTotal, It.Is<int>(x => x == RowContentTemplate + 1 || x == RowContentTemplate + 2)))
                .Returns(grandTotalRow.Object);

            return rowList;
        }

        private IList<Mock<IRow>> CreateRowList()
        {
            var headerRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowHeaderTemplate);
            var bodyRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowContentTemplate);
            var totalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowTotal);
            var grandTotalRow = ExcelTestHelper.MockGetRow(_sheet, ColumnTemplateCount, RowGrandTotal);

            return new List<Mock<IRow>> { headerRow, bodyRow, totalRow, grandTotalRow };
        }

        private PromotionPrizeExcel CreateExcel()
        {
            _excelBuilder = new Mock<IExcelBuilder>();
            var workbook = new Mock<IWorkbook>();
            _excelBuilder.Setup(eb => eb.LoadTemplate(TemplatePath)).Returns(workbook.Object);

            _sheet = new Mock<ISheet>();
            _excelBuilder.Setup(eb => eb.CloneSheet(workbook.Object, null, It.Is<string>(x => x == DefaultSheetName || x == ProductName || x == SabaPromotion))).Returns(_sheet.Object);

            _templateSheet = new Mock<ISheet>();
            workbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(_templateSheet.Object);

            return new PromotionPrizeExcel(_excelBuilder.Object);
        }

        private void AssertRowData(Mock<IRow> row, PromotionPrizeResultModel dataItem)
        {
            AssertCommonCellData(row, dataItem);
        }

        private void AssertCommonCellData(Mock<IRow> row, PromotionPrizeResultModel dataItem)
        {
            var item = dataItem.Data[0];
            Mock.Get(row.Object.Cells[ColumnIndex]).Verify(c => c.SetCellValue(1), Times.Once);
            Mock.Get(row.Object.Cells[ColumnDate]).Verify(c => c.SetCellValue(item.Date.ToString(FormatDate)), Times.Once);
            Mock.Get(row.Object.Cells[ColumnPromotionType]).Verify(c => c.SetCellValue(item.PromotionType), Times.Once);
            Mock.Get(row.Object.Cells[ColumnCurrency]).Verify(c => c.SetCellValue(item.CurrencyName), Times.Once);
            Mock.Get(row.Object.Cells[ColumnExternalId]).Verify(c => c.SetCellValue(item.ExternalId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnSystemId]).Verify(c => c.SetCellValue(item.SystemId), Times.Once);
            Mock.Get(row.Object.Cells[ColumnAmount]).Verify(c => c.SetCellValue(Convert.ToDouble(item.Amount)), Times.Once);
        }
    }
}