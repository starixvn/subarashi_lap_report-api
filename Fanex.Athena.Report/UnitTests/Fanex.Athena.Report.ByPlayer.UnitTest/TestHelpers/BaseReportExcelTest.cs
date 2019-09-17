using System.Collections.Generic;
using System.Linq;
using Moq;
using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers
{
    public class BaseReportExcelTest
    {
        protected BaseReportExcelTest()
        {
        }

        protected static void AssertSetTextRowHeader(IRow row, IEnumerable<string> headers, int startIndex = 0)
        {
            for (int i = 0; i < headers.Count(); i++)
            {
                var mockCell = Mock.Get(row.Cells[i + startIndex]);
                mockCell.Verify(c => c.SetCellValue(headers.ElementAt(i)), Times.Once);
            }
        }

        protected static void AssertHideColumns(Mock<ISheet> mockSheet, params int[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                mockSheet.Verify(s => s.SetColumnWidth(columns[i], 0), Times.Once);
                mockSheet.Verify(s => s.SetColumnHidden(columns[i], true), Times.Once);
            }
        }
    }
}