using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class MatchWinLossResultModel
    {
        public IList<MatchWinLoss> Data { get; set; }

        public IDictionary<int, string> LeagueList { get; set; }

        public IList<MatchWinLoss> SpecialBetTypeResult { get; set; }

        public int ColumnCount { get; set; }

        public MatchWinLoss Total { get; set; }

        public MatchWinLoss GrandTotal { get; set; }
    }
}
