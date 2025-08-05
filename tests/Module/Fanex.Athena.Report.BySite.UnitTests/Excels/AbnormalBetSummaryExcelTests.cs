using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Moq;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class AbnormalBetSummaryExcelTests
    {
        private readonly Mock<IExcelBuilder> _mockExcelBuilder;
        private readonly Mock<IWorkbook> _mockWorkbook;
        private readonly Mock<ISheet> _mockSheet;
        private readonly Mock<ISheet> _mockTemplateSheet;
        private readonly Mock<IRow> _mockRow;
        private readonly Mock<ICell> _mockCell;
        private readonly AbnormalBetSummaryExcel _abnormalBetSummaryExcel;

        public AbnormalBetSummaryExcelTests()
        {
            _mockExcelBuilder = new Mock<IExcelBuilder>();
            _mockWorkbook = new Mock<IWorkbook>();
            _mockSheet = new Mock<ISheet>();
            _mockTemplateSheet = new Mock<ISheet>();
            _mockRow = new Mock<IRow>();
            _mockCell = new Mock<ICell>();
            _abnormalBetSummaryExcel = new AbnormalBetSummaryExcel(_mockExcelBuilder.Object);

            SetupCommonMocks();
        }

        private void SetupCommonMocks()
        {
            _mockExcelBuilder.Setup(b => b.LoadTemplate(It.IsAny<string>())).Returns(_mockWorkbook.Object);
            _mockExcelBuilder.Setup(b => b.CloneSheet(_mockWorkbook.Object, "Template", "Data")).Returns(_mockSheet.Object);
            _mockWorkbook.Setup(w => w.GetSheet("Template")).Returns(_mockTemplateSheet.Object);

            var cells = new List<ICell> { _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object, _mockCell.Object };
            _mockRow.Setup(r => r.Cells).Returns(cells);

            _mockExcelBuilder.Setup(b => b.CopyRow(It.IsAny<ISheet>(), It.IsAny<ISheet>(), It.IsAny<int>(), It.IsAny<int>())).Returns(_mockRow.Object);
        }

        [Fact]
        public void Export_ValidData_ShouldReturnByteArray()
        {
            var metaModel = new AbnormalBetMetaModel
            {
                FromDate = new DateTime(2023, 1, 1),
                ToDate = new DateTime(2023, 1, 31)
            };

            var data = new SortableReportResultModel<AbnormalBetSummary>
            {
                DataList = new SortableList<AbnormalBetSummary>
                {
                    new AbnormalBetSummary
                    {
                        Date = "2023-01-01",
                        Sport = "Football",
                        League = "Premier League",
                        Match = "Team A vs Team B",
                        KOTime = "15:00",
                        Live = "Yes",
                        TotalTransaction = 100,
                        BetType = "1X2",
                        Choice = "1",
                        FinalScore = "2-1"
                    }
                }
            };

            var result = _abnormalBetSummaryExcel.Export("template.xlsx", data, metaModel);

            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public void GetWorkbook_WithData_ShouldCallAssignSheetData()
        {
            var metaModel = new AbnormalBetMetaModel
            {
                FromDate = new DateTime(2023, 1, 1),
                ToDate = new DateTime(2023, 1, 31)
            };

            var data = new SortableReportResultModel<AbnormalBetSummary>
            {
                DataList = new SortableList<AbnormalBetSummary>
                {
                    new AbnormalBetSummary
                    {
                        Date = "2023-01-01",
                        Sport = "Football",
                        League = "Premier League",
                        Match = "Team A vs Team B",
                        KOTime = "15:00",
                        Live = "Yes",
                        TotalTransaction = 100,
                        BetType = "1X2",
                        Choice = "1",
                        FinalScore = "2-1"
                    }
                }
            };

            var result = _abnormalBetSummaryExcel.GetWorkbook("template.xlsx", data, metaModel);

            Assert.NotNull(result);
            _mockExcelBuilder.Verify(b => b.LoadTemplate("template.xlsx"), Times.Once);
            _mockExcelBuilder.Verify(b => b.CloneSheet(_mockWorkbook.Object, "Template", "Data"), Times.Once);
            _mockExcelBuilder.Verify(b => b.RemoveSheet(_mockWorkbook.Object, "Template"), Times.Once);
        }

        [Fact]
        public void GetWorkbook_NoData_ShouldAssignNoInfoRow()
        {
            var metaModel = new AbnormalBetMetaModel
            {
                FromDate = new DateTime(2023, 1, 1),
                ToDate = new DateTime(2023, 1, 31)
            };

            var data = new SortableReportResultModel<AbnormalBetSummary>
            {
                DataList = new SortableList<AbnormalBetSummary>()
            };

            var result = _abnormalBetSummaryExcel.GetWorkbook("template.xlsx", data, metaModel);

            Assert.NotNull(result);
            _mockExcelBuilder.Verify(b => b.SetRowValue(It.IsAny<ISheet>(), It.IsAny<ISheet>(), 5, 2, 11, It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}