namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class PromotionPrize
    {
        public DateTime Date { get; set; }

        public string SystemId { get; set; }

        public string ExternalId { get; set; }

        public decimal Amount { get; set; }

        public decimal Turnover { get; set; }

        public decimal ContributionAmount { get; set; }

        public decimal TipSharing { get; set; }

        public string CurrencyName { get; set; }

        public string PromotionType { get; set; }
    }
}