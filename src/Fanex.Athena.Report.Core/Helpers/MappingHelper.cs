using Fanex.Athena.Report.Resources.Constants;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Helpers
{
    public class MappingHelper
    {
        public static readonly IReadOnlyDictionary<int, int> MapBettypeSportBySport = new Dictionary<int, int>
        {
            { 1204, (int)SportType.Soccer },
            { 1235, (int)SportType.Tennis },
            { 8102, (int)SportType.NumberGameHappy5 }
        };

        public static readonly IReadOnlyDictionary<int, int> MapBettypeSportByProduct = new Dictionary<int, int>
        {
            { 1235, (int)SportType.Tennis },
            { 8102, (int)SportType.NumberGameHappy5 },
            { 2803, (int)SportType.VirtualBasketball },
            { 2804, (int)SportType.VirtualBasketball },
            { 2805, (int)SportType.VirtualBasketball },
            { 2806, (int)SportType.VirtualBasketball }
        };
    }
}
