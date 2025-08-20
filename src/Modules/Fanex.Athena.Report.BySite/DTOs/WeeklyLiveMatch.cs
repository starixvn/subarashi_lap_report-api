namespace Fanex.Athena.Report.BySite.DTOs
{
    using System.Collections.Generic;

    public class WeeklyLiveMatch
    {
        public int SortingWeekValue { get; set; }

        public string SportName { get; set; }

        public int Index { get; set; }

        public IEnumerable<int> NumbersOfMatches { get; set; }
    }
}
