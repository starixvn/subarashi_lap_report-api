namespace Fanex.Athena.Report.BySite.DTOs
{
    using System.Collections.Generic;

    public class LiveReportByBetType
    {
        public string ProductName { get; set; }

        public IList<LiveReport> LiveReports { get; set; }
    }
}
