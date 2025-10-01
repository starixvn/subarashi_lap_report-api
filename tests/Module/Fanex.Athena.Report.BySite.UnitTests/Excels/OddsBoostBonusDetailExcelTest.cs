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

    public class OddsBoostBonusDetailExcelTest
    {
        private const string TemplateSheetName = "Template";
        private const string SheetName = "Data";
        private const int ColumnIndex = 0;
        private const int ColumnDate = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnRefNo = 4;
        private const int ColumnProduct = 5;
        private const int ColumnPromotionType = 6;
        private const int ColumnCustomerAmount = 7;
        private const int RowNoInfoTemplate = 6;
        private const int ColumnTemplateCount = 8;
        private const int RowHeaderTemplate = 1;
        private const int RowTitleTemplate = 0;
        private const int RowContentTemplate = 3;
        private const int RowGrantTotalTemplate = 5;

        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Export_WhenCall_ReturnByteArray()
        {
            // Arrange
            var template = _fixture.Create<string>();
            var data = _fixture.Create<SortableReportResultModel<OddsBoostBonusDetail>>();
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();

            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockTemplateSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();

            mockExcelBuilder.Setup(b => b.LoadTemplate(template)).Returns(mockWorkbook.Object);
            mockExcelBuilder.Setup(b => b.CloneSheet(mockWorkbook.Object, It.IsAny<string>(), It.IsAny<string>())).Returns(mockSheet.Object);
            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, It.IsAny<int>(), It.IsAny<int>())).Returns(mockRow.Object);
            mockWorkbook.Setup(w => w.GetSheet(It.IsAny<string>())).Returns(mockTemplateSheet.Object);
            mockSheet.Setup(s => s.CreateRow(It.IsAny<int>())).Returns(mockRow.Object);
            mockSheet.Setup(s => s.GetRow(It.IsAny<int>())).Returns(mockRow.Object);
            var cells = new List<ICell>();
            for (int i = 0; i < 8; i++) // Create 8 cells for the header text list
            {
                var cell = new Mock<ICell>();
                cell.Setup(c => c.SetCellValue(It.IsAny<string>()));
                cells.Add(cell.Object);
            }
            mockRow.Setup(r => r.Cells).Returns(cells);
            mockWorkbook.Setup(w => w.Write(It.IsAny<System.IO.Stream>(), It.IsAny<bool>()));

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            var result = excel.Export(template, data, metaModel);

            // Assert
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public void GetWorkbook_WithData_AssignSheetData()
        {
            // Arrange
            var template = _fixture.Create<string>();
            var dataList = new SortableList<OddsBoostBonusDetail>
            {
                new()
                {
                    Date = new DateTime(2022, 8, 17),
                    DateString = "08/17/2022",
                    ExternalId = "qatest03",
                    SystemId = "Athena000UUS01003",
                    RefNo = "437934648123719683",
                    ProductCode = "SB",
                    ProductName = "Sportsbook",
                    PromotionType = "Sportsbook - Odds Boost Bonus",
                    CustomerAmount = 4.08m
                }
            };
            var data = new SortableReportResultModel<OddsBoostBonusDetail>
            {
                DataList = dataList,
                TotalData = new OddsBoostBonusDetail { CustomerAmount = 4.08m }
            };
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();

            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockTemplateSheet = new Mock<ISheet>();

            mockExcelBuilder.Setup(b => b.LoadTemplate(template)).Returns(mockWorkbook.Object);
            mockExcelBuilder.Setup(b => b.CloneSheet(mockWorkbook.Object, TemplateSheetName, SheetName)).Returns(mockSheet.Object);
            mockWorkbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(mockTemplateSheet.Object);

            var mockHeaderRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, RowHeaderTemplate);
            var mockContentRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, RowContentTemplate);
            var mockTotalRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, RowContentTemplate + 1);

            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(mockHeaderRow.Object);
            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, RowContentTemplate, RowContentTemplate))
                .Returns(mockContentRow.Object);
            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, RowGrantTotalTemplate, RowContentTemplate + 1))
                .Returns(mockTotalRow.Object);

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            var result = excel.GetWorkbook(template, data, metaModel);

            // Assert
            Assert.Equal(mockWorkbook.Object, result);
            mockExcelBuilder.Verify(b => b.RemoveSheet(mockWorkbook.Object, TemplateSheetName), Times.Once);
        }

        [Fact]
        public void GetWorkbook_NoData_AssignNoInfoRow()
        {
            // Arrange
            var template = _fixture.Create<string>();
            var data = new SortableReportResultModel<OddsBoostBonusDetail>
            {
                DataList = new SortableList<OddsBoostBonusDetail>(),
                TotalData = new OddsBoostBonusDetail()
            };
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();

            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockTemplateSheet = new Mock<ISheet>();

            mockExcelBuilder.Setup(b => b.LoadTemplate(template)).Returns(mockWorkbook.Object);
            mockExcelBuilder.Setup(b => b.CloneSheet(mockWorkbook.Object, TemplateSheetName, SheetName)).Returns(mockSheet.Object);
            mockWorkbook.Setup(w => w.GetSheet(TemplateSheetName)).Returns(mockTemplateSheet.Object);

            var mockHeaderRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, RowHeaderTemplate);
            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(mockHeaderRow.Object);

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            excel.GetWorkbook(template, data, metaModel);

            // Assert
            mockExcelBuilder.Verify(b => b.SetRowValue(mockSheet.Object, mockTemplateSheet.Object, RowNoInfoTemplate, RowContentTemplate, ColumnTemplateCount, ViewResources.NoInfo), Times.Once);
        }

        [Fact]
        public void AssignHeader_WhenCall_SetCorrectHeaders()
        {
            // Arrange
            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockSheet = new Mock<ISheet>();
            var mockTemplateSheet = new Mock<ISheet>();
            var mockRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, RowHeaderTemplate);

            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, RowHeaderTemplate, RowHeaderTemplate))
                .Returns(mockRow.Object);

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            excel.GetType().GetMethod("AssignHeader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(excel, new object[] { mockSheet.Object, mockTemplateSheet.Object });

            // Assert
            var expectedHeaders = new[]
            {
                "#",
                ViewResources.Date,
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.RefNo,
                ViewResources.Product,
                ViewResources.PromotionType,
                ViewResources.CustomerAmount
            };

            ExcelTestHelper.AssertSetTextRowHeader(mockRow.Object, expectedHeaders.ToList());
        }

        [Fact]
        public void AssignCellData_WhenCall_SetCorrectCellValues()
        {
            // Arrange
            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, 0);

            var item = new OddsBoostBonusDetail
            {
                DateString = "08/17/2022",
                ExternalId = "qatest03",
                SystemId = "Athena000UUS01003",
                RefNo = "437934648123719683",
                ProductName = "Sportsbook",
                PromotionType = "Sportsbook - Odds Boost Bonus",
                CustomerAmount = 4.08m
            };

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            var method = excel.GetType().GetMethod("AssignCellData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, new object[] { mockRow.Object, item });

            // Assert
            var mockCells = mockRow.Object.Cells.Select(Mock.Get).ToArray();
            mockCells[ColumnDate].Verify(c => c.SetCellValue("08/17/2022"), Times.Once);
            mockCells[ColumnExternalId].Verify(c => c.SetCellValue("qatest03"), Times.Once);
            mockCells[ColumnSystemId].Verify(c => c.SetCellValue("Athena000UUS01003"), Times.Once);
            mockCells[ColumnRefNo].Verify(c => c.SetCellValue("437934648123719683"), Times.Once);
            mockCells[ColumnProduct].Verify(c => c.SetCellValue("Sportsbook"), Times.Once);
            mockCells[ColumnPromotionType].Verify(c => c.SetCellValue("Sportsbook - Odds Boost Bonus"), Times.Once);
            mockCells[ColumnCustomerAmount].Verify(c => c.SetCellValue(4.08), Times.Once);
        }

        [Fact]
        public void AssignTitle_WhenCall_SetCorrectTitle()
        {
            // Arrange
            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockSheet = new Mock<ISheet>();
            var mockTemplateSheet = new Mock<ISheet>();

            var metaModel = new ExtraPayoutMetaModel
            {
                CurrencyName = "USD",
                Username = "qatest03",
                DrillDownFromDate = new DateTime(2022, 8, 1),
                DrillDownToDate = new DateTime(2022, 8, 17),
                DrillDownLevel = (int)ReportLevel.Main
            };

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            var method = excel.GetType().GetMethod("AssignTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(excel, new object[] { mockSheet.Object, mockTemplateSheet.Object, metaModel });

            // Assert
            var expectedTitle = $"{ViewResources.PromotionDetails}, 08/01/2022 -> 08/17/2022 - {ViewResources.OddsBoostBonus} - View in USD";
            mockExcelBuilder.Verify(b => b.SetRowValue(
                mockSheet.Object,
                mockTemplateSheet.Object,
                RowTitleTemplate,
                RowTitleTemplate,
                ColumnTemplateCount,
                expectedTitle), Times.Once);

            mockSheet.Verify(s => s.CreateFreezePane(0, RowContentTemplate), Times.Once);
            mockExcelBuilder.Verify(b => b.ClearTemplate(mockSheet.Object, RowContentTemplate), Times.Once);
        }

        [Fact]
        public void AssignGrandTotal_WhenCall_SetCorrectTotalRow()
        {
            // Arrange
            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockSheet = new Mock<ISheet>();
            var mockTemplateSheet = new Mock<ISheet>();
            var mockRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, 5);

            var data = new SortableReportResultModel<OddsBoostBonusDetail>
            {
                TotalData = new OddsBoostBonusDetail { CustomerAmount = 59.63m }
            };

            var metaModel = new ExtraPayoutMetaModel { BaseCurrency = "USD" };

            mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockTemplateSheet.Object, RowGrantTotalTemplate, 5))
                .Returns(mockRow.Object);

            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            var method = excel.GetType().GetMethod("AssignGrandTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(excel, new object[] { mockSheet.Object, mockTemplateSheet.Object, data, 5, metaModel });

            // Assert
            var mockIndexCell = Mock.Get(mockRow.Object.Cells[ColumnIndex]);
            mockIndexCell.Verify(c => c.SetCellValue(string.Format(ViewResources.Totalin, "USD")), Times.Once);

            var mockAmountCell = Mock.Get(mockRow.Object.Cells[ColumnCustomerAmount]);
            mockAmountCell.Verify(c => c.SetCellValue(59.63), Times.Once);

            mockExcelBuilder.Verify(b => b.MergeCells(mockSheet.Object, 5, 5, ColumnIndex, ColumnPromotionType), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        public void AssignCellData_CustomerAmountVariousValues_SetCorrectValues(decimal amount)
        {
            // Arrange
            var mockExcelBuilder = new Mock<IExcelBuilder>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = ExcelTestHelper.MockGetRow(mockSheet, ColumnTemplateCount, 0);

            var item = new OddsBoostBonusDetail { CustomerAmount = amount };
            var excel = new OddsBoostBonusDetailExcel(mockExcelBuilder.Object);

            // Act
            var method = excel.GetType().GetMethod("AssignCellData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, new object[] { mockRow.Object, item });

            // Assert
            var mockAmountCell = Mock.Get(mockRow.Object.Cells[ColumnCustomerAmount]);
            mockAmountCell.Verify(c => c.SetCellValue((double)amount), Times.Once);
        }
    }
}