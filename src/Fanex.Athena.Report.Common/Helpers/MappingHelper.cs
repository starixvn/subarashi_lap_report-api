namespace Fanex.Athena.Report.Common.Helpers
{
    using System.Collections.Generic;
    using Resources.Constants;

    public static class MappingHelper
    {
        public static readonly IReadOnlyDictionary<int, int> MapBettypeSportBySport = new Dictionary<int, int>
        {
            { 1204, SportType.Soccer },
            { 1235, SportType.Tennis },
            { 8102, SportType.NumberGameHappy5 }
        };

        public static readonly IReadOnlyDictionary<int, int> MapBettypeSportByProduct = new Dictionary<int, int>
        {
            { 1235, SportType.Tennis },
            { 8102, SportType.NumberGameHappy5 },
            { 2803, SportType.VirtualBasketball },
            { 2804, SportType.VirtualBasketball },
            { 2805, SportType.VirtualBasketball },
            { 2806, SportType.VirtualBasketball }
        };
    }
}
