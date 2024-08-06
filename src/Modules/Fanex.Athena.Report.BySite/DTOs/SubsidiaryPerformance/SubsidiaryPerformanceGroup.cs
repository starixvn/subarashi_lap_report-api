using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance
{
    public class SubsidiaryPerformanceGroup
    {
        public string WhiteLabel { get; set; }

        public int TotalItem { get; set; }

        public IEnumerable<SubsidiaryPerformanceSummary> Details { get; set; }
    }
}