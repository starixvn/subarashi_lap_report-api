using System.Collections.Generic;
using Moq;
using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers
{
    public static class ExcelTestHelper
    {
        public static void MockGetRow(Mock<ISheet> sheet, int columnCount)
        {
            var row = new Mock<IRow>();
            var cell = new Mock<ICell>();
            cell.Setup(c => c.Row).Returns(row.Object);
            var cellList = new List<ICell>();
            for (int i = 0; i < columnCount; i++)
            {
                cellList.Add(cell.Object);
            }

            row.Setup(r => r.Cells).Returns(cellList);
            sheet.Setup(s => s.GetRow(0)).Returns(row.Object);
        }

        public static Mock<IRow> MockGetRow(Mock<ISheet> sheet, int columnCount, int destRowIndex)
        {
            var row = new Mock<IRow>();
            var cellList = new List<ICell>(columnCount);

            for (int i = 0; i < columnCount; i++)
            {
                var cell = new Mock<ICell>();
                cellList.Add(cell.Object);
            }

            row.Setup(r => r.Cells).Returns(cellList);
            sheet.Setup(s => s.GetRow(destRowIndex)).Returns(row.Object);

            return row;
        }

        public static ICell MockGetCell(ISheet sheet, int rowIndex, int columnIndex)
        {
            return sheet.GetRow(rowIndex).GetCell(columnIndex);
        }
    }
}