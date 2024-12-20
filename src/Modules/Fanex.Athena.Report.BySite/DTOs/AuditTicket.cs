namespace Fanex.Athena.Report.BySite.DTOs
{
    using Newtonsoft.Json;
    using System;

    public class AuditTicket
    {
        private const string DateTimeFormat = "yyyy-MM-dd hh:mm:ss tt";

        public int Index { get; set; }

        public string PlayerName { get; set; }

        public string CurrencyName { get; set; }

        public string RefNo { get; set; }

        [JsonIgnore]
        public DateTime TransTime { get; set; }

        public string TransTimeString
        {
            get { return TransTime.ToString(DateTimeFormat); }
        }

        [JsonIgnore]
        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string SportName { get; set; }

        public string BetType { get; set; }

        public decimal Turnover { get; set; }

        public decimal Payout { get; set; }

        public decimal WinLoss { get; set; }

        public decimal TurnoverBased { get; set; }

        public decimal PayoutBased { get; set; }

        public decimal WinLossBased { get; set; }

        public string Status { get; set; }

        [JsonIgnore]
        public int StatusId { get; set; }
    }
}