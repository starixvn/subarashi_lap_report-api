using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.Core.Extensions
{
    public static class GuardExtension
    {
        public static bool IsPresent<T>(this IEnumerable<T> source, Func<T, bool> predicate = null)
          => source != null && (predicate == null ? source.Any() : source.Any(predicate));
    }
}