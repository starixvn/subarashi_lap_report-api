using System.Collections.Generic;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DTO;

namespace Fanex.Athena.Report.Common.Excel
{
    public static class ExcelTitleBuilder
    {
        public static string GetTitleCurrencyName(string viewInPattern, bool isConvert, string baseCurrency, string currencyName)
        {
            var displayedCurrency = isConvert ? baseCurrency?.ToUpperInvariant() : currencyName;

            return string.Format(viewInPattern, displayedCurrency);
        }

        public static string GetLicenseeWLProduct(IEnumerable<ProductOption> productOptions, string licenseeWlText, string caption = null)
        {
            var productCodeList = productOptions.Where(p => p.Status)
                            .Aggregate(string.Empty, (current, p) => current + (string.IsNullOrEmpty(current) ? p.Code : ", " + p.Code));

            if (string.IsNullOrEmpty(productCodeList))
            {
                return string.IsNullOrWhiteSpace(caption) ? string.Empty : ", " + caption;
            }

            return string.Format(", {0} ({1})", string.IsNullOrWhiteSpace(caption) ? licenseeWlText : caption, productCodeList);
        }
    }
}