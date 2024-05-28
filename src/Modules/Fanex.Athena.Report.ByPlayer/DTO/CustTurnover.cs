using System;

namespace Fanex.Athena.Report.ByPlayer.DTO
{
    public class CustTurnover
    {
        public int Index { get; set; }

        public DateTime ReportBy { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public int RowSpan { get; set; }

        public int RowSpan2 { get; set; }

        public string Currency { get; set; }

        public int SportId { get; set; }

        public string SportType { get; set; }

        public int TotalTransactions { get; set; }

        public string Bettype { get; set; }

        public decimal EffectiveBettingValue { get; set; }

        public decimal Turnover { get; set; }

        public decimal TotalTurnover { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal CustomerTotal => CustomerWinLoss + CustomerComm;
    }
}
