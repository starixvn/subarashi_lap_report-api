using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class WeeklyLiveMatch
    {
        public int SortingWeekValue { get; set; }

        public string SportName { get; set; }

        public int Index { get; set; }

        public IEnumerable<int> NumbersOfMatches { get; set; }
    }
}
