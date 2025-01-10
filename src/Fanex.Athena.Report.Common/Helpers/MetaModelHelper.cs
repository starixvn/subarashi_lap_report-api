using Fanex.Athena.Report.Common.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.Common.Helpers
{
    public static class MetaModelHelper
    {
        public static string ToSelectedProductCodeList(this IEnumerable<ProductOption> productOptions, Predicate<ProductOption> predicate = null)
            => string.Join(",", productOptions
                               .Where(_ => _.Status && (predicate == null || predicate.Invoke(_)))
                               .Select(_ => _.Code)
            );
    }
}
