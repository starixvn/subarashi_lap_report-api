using System.Collections.Generic;
using System.IO;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Extensions;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Excels
{
    public class AbnormalBetDetailExcelTests
    {
        private readonly Mock<IExcelBuilder> _mockExcelBuilder;
        private readonly AbnormalBetDetailExcel _abnormalBetDetailExcel;

        public AbnormalBetDetailExcelTests()
        {
            _mockExcelBuilder = new Mock<IExcelBuilder>();
            _abnormalBetDetailExcel = new AbnormalBetDetailExcel(_mockExcelBuilder.Object);
        }

        [Fact]
        public void Export_ShouldReturnByteArray()
        {
            // Arrange
            var template = "template";
            var data = new SortableReportResultModel<AbnormalBetDetail>
            {
                DataList = new SortableList<AbnormalBetDetail>
                {
                    new AbnormalBetDetail { IP = "127.0.0.1", Location = "Local" }
                }
            };
            var metaModel = new AbnormalBetMetaModel();

            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();

            _mockExcelBuilder.Setup(b => b.LoadTemplate(template)).Returns(mockWorkbook.Object);
            _mockExcelBuilder.Setup(b => b.CloneSheet(mockWorkbook.Object, It.IsAny<string>(), It.IsAny<string>())).Returns(mockSheet.Object);
            mockWorkbook.Setup(w => w.GetSheet(It.IsAny<string>())).Returns(mockSheet.Object);
            _mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockSheet.Object, It.IsAny<int>(), It.IsAny<int>())).Returns(mockRow.Object);
            mockRow.Setup(r => r.Cells).Returns(new List<ICell> { mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object });

            mockWorkbook.Setup(w => w.Write(It.IsAny<MemoryStream>()));

            // Act
            var result = _abnormalBetDetailExcel.Export(template, data, metaModel);

            // Assert
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public void GetWorkbook_ShouldReturnWorkbook()
        {
            // Arrange
            var template = "template";
            var data = new SortableReportResultModel<AbnormalBetDetail>
            {
                DataList = new SortableList<AbnormalBetDetail>
                {
                    new AbnormalBetDetail { IP = "127.0.0.1", Location = "Local" }
                }
            };
            var metaModel = new AbnormalBetMetaModel();

            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();

            _mockExcelBuilder.Setup(b => b.LoadTemplate(template)).Returns(mockWorkbook.Object);
            _mockExcelBuilder.Setup(b => b.CloneSheet(mockWorkbook.Object, It.IsAny<string>(), It.IsAny<string>())).Returns(mockSheet.Object);
            mockWorkbook.Setup(w => w.GetSheet(It.IsAny<string>())).Returns(mockSheet.Object);
            _mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockSheet.Object, It.IsAny<int>(), It.IsAny<int>())).Returns(mockRow.Object);
            mockRow.Setup(r => r.Cells).Returns(new List<ICell> { mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object });

            // Act
            var result = _abnormalBetDetailExcel.GetWorkbook(template, data, metaModel);

            // Assert
            Assert.IsAssignableFrom<IWorkbook>(result);
        }

        [Fact]
        public void GetWorkbook_NoData_ShouldAssignNoInfoRow()
        {
            // Arrange
            var template = "template";
            var data = new SortableReportResultModel<AbnormalBetDetail>
            {
                DataList = new SortableList<AbnormalBetDetail>()
            };
            var metaModel = new AbnormalBetMetaModel();

            var mockWorkbook = new Mock<IWorkbook>();
            var mockSheet = new Mock<ISheet>();
            var mockRow = new Mock<IRow>();
            var mockCell = new Mock<ICell>();

            _mockExcelBuilder.Setup(b => b.LoadTemplate(template)).Returns(mockWorkbook.Object);
            _mockExcelBuilder.Setup(b => b.CloneSheet(mockWorkbook.Object, It.IsAny<string>(), It.IsAny<string>())).Returns(mockSheet.Object);
            mockWorkbook.Setup(w => w.GetSheet(It.IsAny<string>())).Returns(mockSheet.Object);
            _mockExcelBuilder.Setup(b => b.CopyRow(mockSheet.Object, mockSheet.Object, It.IsAny<int>(), It.IsAny<int>())).Returns(mockRow.Object);
            mockRow.Setup(r => r.Cells).Returns(new List<ICell> { mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object, mockCell.Object });

            // Act
            _abnormalBetDetailExcel.GetWorkbook(template, data, metaModel);

            // Assert
            _mockExcelBuilder.Verify(b => b.SetRowValue(mockSheet.Object, mockSheet.Object, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
