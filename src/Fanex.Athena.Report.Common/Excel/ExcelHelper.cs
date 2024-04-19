using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Resources;
using NPOI.HSSF.UserModel;
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

        public static string GetTitleCurrencyName(IConvertingOption convertingOption)
        {
            return convertingOption.IsConvert
                ? string.Format(ViewResources.Viewin, convertingOption.BaseCurrency.ToUpperInvariant())
                : string.Format(ViewResources.Viewin, convertingOption.CurrencyName);
        }

        public static string GetLicenseeWlProduct(IEnumerable<ProductOption> productOptions, string caption = null)
        {
            var productCodeList = productOptions.Where(p => p.Status)
                                                .Aggregate(string.Empty, (current, p) => current + (string.IsNullOrEmpty(current) ? p.Code : ", " + p.Code));

            if (string.IsNullOrEmpty(productCodeList))
            {
                return string.IsNullOrWhiteSpace(caption) ? string.Empty : ", " + caption;
            }

            return
                $", {(string.IsNullOrWhiteSpace(caption) ? ViewResources.LicenseeW_L : caption)} ({productCodeList})";
        }

        public static byte[] ExportEmptyFile()
        {
            var templateWorkbook = new HSSFWorkbook();
            templateWorkbook.CreateSheet();
            var ms = new MemoryStream();
            templateWorkbook.Write(ms);

            return ms.ToArray();
        }
    }
}