using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class ExtraPayoutDetailResultModel
    {
        public List<ExtraPayoutDetail> Data { get; set; }

        public ExtraPayoutDetail TotalSummary { get; set; }
    }
}