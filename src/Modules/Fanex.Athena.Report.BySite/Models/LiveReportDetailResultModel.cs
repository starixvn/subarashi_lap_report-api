using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class LiveReportDetailResultModel
    {
        public IEnumerable<LiveReport> DataByCurrency { get; set; }

        public IEnumerable<LiveReportBySport> DataBySport { get; set; }

        public IEnumerable<LiveReportByBetType> DataByBetType { get; set; }

        public bool IsShowedDetailLink { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }
    }
}
