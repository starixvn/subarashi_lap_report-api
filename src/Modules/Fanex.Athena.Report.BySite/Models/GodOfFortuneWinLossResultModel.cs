using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;

namespace Fanex.Athena.Report.BySite.Models
{
    public class GodOfFortuneWinLossResultModel
    {
        public List<GodOfFortuneWinLoss> Data { get; set; }

        public List<GodOfFortuneWinLoss> Total { get; set; }

        public Dictionary<string, GodOfFortuneWinLoss> SummaryTotal { get; set; }
    }
}