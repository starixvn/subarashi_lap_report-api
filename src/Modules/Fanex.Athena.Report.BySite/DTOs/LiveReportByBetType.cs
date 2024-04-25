using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class LiveReportByBetType
    {
        public string ProductName { get; set; }

        public IList<LiveReport> LiveReports { get; set; }
    }
}
