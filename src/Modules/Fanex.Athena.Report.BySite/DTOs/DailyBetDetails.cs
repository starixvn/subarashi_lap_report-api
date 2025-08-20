namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;
    using Newtonsoft.Json;

    public class DailyBetDetails
    {
        private const string DateTimeFormat = "MM/dd/yyyy";

        public string RIdx { get; set; }

        public string BrandUrl { get; set; }

        public string GameType { get; set; }

        public string CurrencyName { get; set; }

        [JsonIgnore]
        public DateTime SettlementDate { get; set; }

        public string SettlementDateString
        {
            get { return SettlementDate.ToString(DateTimeFormat); }
        }

        public string RefNo { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal BetAmount { get; set; }

        public decimal NetAmount { get; set; }

        public decimal CreditAmount => BetAmount - NetAmount;
    }
}