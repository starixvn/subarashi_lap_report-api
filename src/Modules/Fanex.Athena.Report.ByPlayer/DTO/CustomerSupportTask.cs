using System;

namespace Fanex.Athena.Report.ByPlayer.DTO
{
    public class CustomerSupportTask
    {
        protected CustomerSupportTask()
        { }

        public int Index { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public string Currency { get; set; }
    }

    public class CustomerClosingBalance : CustomerSupportTask
    {
        public decimal ClosingBalance { get; set; }
    }

    public class CustomerRunningTicket : CustomerSupportTask
    {
        public string RefNo { get; set; }

        public string TransId { get; set; }

        public string BetType { get; set; }

        public string Status { get; set; }

        public DateTime TransTime { get; set; }

        public DateTime WinlostDate { get; set; }

        public decimal Stake { get; set; }

        public decimal ActualStake { get; set; }
    }
}