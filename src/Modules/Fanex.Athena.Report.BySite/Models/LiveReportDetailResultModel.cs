namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class LiveReportDetailResultModel
    {
        public IEnumerable<LiveReport> DataByCurrency { get; set; }

        public IEnumerable<LiveReportBySport> DataBySport { get; set; }

        public IEnumerable<LiveReportByBetType> DataByBetType { get; set; }

        public bool IsShowedDetailLink { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }
    }
}
