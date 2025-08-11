namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class ExtraPayoutDetailResultModel
    {
        public List<ExtraPayoutDetail> Data { get; set; }

        public ExtraPayoutDetail TotalSummary { get; set; }
    }
}