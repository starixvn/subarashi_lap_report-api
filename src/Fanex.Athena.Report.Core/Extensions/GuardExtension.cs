namespace Fanex.Athena.Report.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class GuardExtension
    {
        public static bool IsPresent<T>(this IEnumerable<T> source, Func<T, bool> predicate = null)
          => source != null && (predicate == null ? source.Any() : source.Any(predicate));
    }
}