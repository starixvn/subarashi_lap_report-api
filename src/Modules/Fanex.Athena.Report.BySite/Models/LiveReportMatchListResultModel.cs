using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.Common.Extensions;
using Newtonsoft.Json;

namespace Fanex.Athena.Report.BySite.Models
{
    public class LiveReportMatchListResultModel
    {
        public SortableList<LiveReportWinLossByMatch> Data { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }

        public bool HasAmountDueToColumn { get; set; }

        public bool IsAllowBa { get; set; }
    }
}
