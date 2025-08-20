namespace Fanex.Athena.Report.BySite.Models
{
    using Common.Extensions;
    using DTOs;

    public class LiveReportMatchListResultModel
    {
        public SortableList<LiveReportWinLossByMatch> Data { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }

        public bool HasAmountDueToColumn { get; set; }
    }
}