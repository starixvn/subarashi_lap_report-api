using System.Linq;
using Fanex.Athena.Report.Common.Excel;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Excel
{
    public class ExcelHelperTest
    {
        [Fact]
        public void SetTextRowHeader_WhenCall_CellIsSetValueCorrectly()
        {
            // Arrange
            var headerText = new[]
            {
                "Header1",
                "Header2",
            };

            var cell = new Mock<ICell>();
            var row = new Mock<IRow>();
            row.Setup(r => r.Cells).Returns(Enumerable.Repeat(cell.Object, 2).ToList());

            // Action
            ExcelHelper.SetTextRowHeader(row.Object, headerText);

            // Assert
            cell.Verify(c => c.SetCellValue(headerText[0]), Times.Once);
            cell.Verify(c => c.SetCellValue(headerText[1]), Times.Once);
        }

        [Theory]
        [InlineData(100, 10, 10)]
        [InlineData(101, 10, 11)]
        [InlineData(95, 10, 10)]
        public void GetNumberOfSheets_WhenCall_ReturnCorrectTotalSheet(int totalRow, int pageSize, int expected)
        {
            // Action
            var result = ExcelHelper.GetNumberOfSheets(totalRow, pageSize);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}