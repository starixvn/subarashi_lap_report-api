namespace Fanex.Athena.Report.Core.Models.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RiskLevel
    {
        public const string Uncategorized = "Uncategorized";

        private static readonly IDictionary<string, int> RiskLevelMapper =
            new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "VIP", 1 },
                { "Healthy", 2 },
                { "New", 3 },
                { "Moderate", 4 },
                { "Suspicious", 5 },
                { "Dangerous", 6 },
                { "Problems", 7 },
                { "Robot", 8 },
                { Uncategorized, 999 }
            };

        public string Id { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> CustomerClassList { get; set; } = Enumerable.Empty<string>();

        public static int GetComparerValue(RiskLevel level)
        {
            const int DefaultValue = 99;

            return RiskLevelMapper.TryGetValue(level.Key, out var value) ? value : DefaultValue;
        }
    }

    public class RiskLevelComparer : IComparer<RiskLevel>
    {
        public int Compare(RiskLevel x, RiskLevel y)
        {
            var xValue = RiskLevel.GetComparerValue(x);
            var yValue = RiskLevel.GetComparerValue(y);

            if (xValue > yValue)
            {
                return 1;
            }

            if (xValue < yValue)
            {
                return -1;
            }

            return 0;
        }
    }
}
