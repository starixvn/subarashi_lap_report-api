namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class AuditTicket
    {
        public int Index { get; set; }

        public string PlayerName { get; set; }

        public string CurrencyName { get; set; }

        public long RefNo { get; set; }

        public DateTime TransTime { get; set; }

        public string ProductCode { get; set; }

        public string BetType { get; set; }

        public decimal Turnover { get; set; }

        public decimal WinLoss { get; set; }

        public string Status { get; set; }
    }
}
