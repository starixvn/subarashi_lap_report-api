namespace Fanex.Athena.Report.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class LinqExtension
    {
#pragma warning disable S2436 // Types and methods should not have too many generic parameters

        public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> leftKey, Func<TRight, TKey> rightKey,
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
            Func<TLeft, TRight, TResult> result)
        {
            return left.GroupJoin(right, leftKey, rightKey, (l, r) => new { l, r })
                 .SelectMany(
                     o => o.r.DefaultIfEmpty(),
                     (l, r) => new { lft = l.l, rght = r })
                 .Select(o => result.Invoke(o.lft, o.rght));
        }
    }
}