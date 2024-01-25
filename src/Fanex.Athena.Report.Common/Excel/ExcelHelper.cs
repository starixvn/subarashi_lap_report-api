using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.Common.Excel
{
    public static class ExcelHelper
    {
        public static void SetTextRowHeader(IRow row, IEnumerable<string> headerTextList)
        {
            for (int i = 0; i < headerTextList.Count(); i++)
            {
                row.Cells[i].SetCellValue(headerTextList.ElementAt(i));
            }
        }

        public static int GetNumberOfSheets(int totalRows, int pageSize)
        {
            return totalRows % pageSize == 0 ? totalRows / pageSize : (totalRows / pageSize) + 1;
        }
    }
}