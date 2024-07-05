namespace Fanex.Athena.Report.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model.DTO;

    public static class MetaModelHelper
    {
        public static bool ValidateBaseCurrency(string baseCurrency, bool isAllowEmpty = true)
            => isAllowEmpty || !string.IsNullOrEmpty(baseCurrency);

        public static string ToSelectedProductCodeList(this IEnumerable<ProductOption> productOptions, Predicate<ProductOption> predicate = null)
            => string.Join(",", productOptions
                .Where(_ => _.Status && (predicate == null || predicate.Invoke(_)))
                .Select(_ => _.Code));
    }
}