namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using Fanex.Athena.Report.BySite.DTOs;

    public class PromotionPrizeResultModel
    {
        public IList<PromotionPrize> Data { get; set; }

        public IList<PromotionPrize> SummaryTotal { get; set; }

        public IList<CurrencyTotal> CurrencyTotal { get; set; }

        public decimal TurnoverTotal { get; set; }

        public decimal ContributionAmountTotal { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal TipTotal { get; set; }
    }
}