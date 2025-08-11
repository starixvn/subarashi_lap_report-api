namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs.SubsidiaryPerformance;

    public class SubsidiaryPerformanceResultModel
    {
        public IEnumerable<SubsidiaryPerformanceGroup> Data { get; set; }

        public IDictionary<string, SubsidiaryPerformanceBase> Total { get; set; }

        public SubsidiaryPerformanceBase GrandTotal { get; set; }
    }
}