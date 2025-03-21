using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance;

namespace Fanex.Athena.Report.BySite.Models
{
    public class SubsidiaryPerformanceResultModel
    {
        public IEnumerable<SubsidiaryPerformanceGroup> Data { get; set; }

        public IDictionary<string, SubsidiaryPerformanceBase> Total { get; set; }

        public SubsidiaryPerformanceBase GrandTotal { get; set; }
    }
}