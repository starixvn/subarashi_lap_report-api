namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class GodOfFortuneWinLossResultModel
    {
        public List<GodOfFortuneWinLoss> Data { get; set; }

        public List<GodOfFortuneWinLoss> Total { get; set; }

        public Dictionary<string, GodOfFortuneWinLoss> SummaryTotal { get; set; }
    }
}