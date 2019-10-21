namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using Fanex.Athena.Report.BySite.DTOs;

    public class PromotionPrizeResultModel
    {
        public IList<PromotionPrize> Data { get; set; }

        public IDictionary<string, PromotionPrize> SummaryTotal { get; set; }
    }
}
