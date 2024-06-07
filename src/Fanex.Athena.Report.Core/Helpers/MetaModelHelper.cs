using System;
using System.Collections.Generic;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DTO;

namespace Fanex.Athena.Report.Core.Helpers
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
