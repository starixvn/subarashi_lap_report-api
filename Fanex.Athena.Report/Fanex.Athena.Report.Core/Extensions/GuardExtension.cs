using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.Core.Extensions
{
    public static class GuardExtension
    {
        public static bool IsPresent<T>(this IEnumerable<T> source)
            => source != null && source.Any();
    }
}