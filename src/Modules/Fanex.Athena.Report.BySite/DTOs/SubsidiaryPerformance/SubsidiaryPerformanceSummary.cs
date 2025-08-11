namespace Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance
{
    using System.Collections.Generic;

    public class SubsidiaryPerformanceSummary
    {
        public string Subsidiary { get; set; }

        public int TotalItem { get; set; }

        public IDictionary<string, IEnumerable<SubsidiaryPerformanceBase>> Details { get; set; }
    }
}