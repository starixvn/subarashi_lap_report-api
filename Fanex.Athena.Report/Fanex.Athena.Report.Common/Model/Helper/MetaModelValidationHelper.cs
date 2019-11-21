using System;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DataDomain;

namespace Fanex.Athena.Report.Common.Model.Helper
{
    public static class MetaModelValidationHelper
    {
        public static bool ValidateBaseCurrency(string baseCurrency, bool isAllowEmpty = true)
        {
            return (isAllowEmpty && string.IsNullOrEmpty(baseCurrency)) || SystemBaseCurrency.BaseCurrencyList
           .Any(x => x.Value.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase));
        }
    }
}