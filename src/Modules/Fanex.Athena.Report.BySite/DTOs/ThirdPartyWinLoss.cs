using System;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class ThirdPartyWinLoss
    {
        public int RowSpan { get; set; }

        public int Index { get; set; }

        public DateTime? Date { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public int CustomerId { get; set; }

        public string CurrencyName { get; set; }

        public int CurrencyId { get; set; }

        public decimal TurnOver { get; set; }

        public decimal TotalTransaction { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal LicenseeWL { get; set; }

        public decimal ContributionWL { get; set; }

        public decimal PromotionWL { get; set; }

        public decimal JackpotWL { get; set; }

        public decimal TotalWL { get; set; }
    }
}
