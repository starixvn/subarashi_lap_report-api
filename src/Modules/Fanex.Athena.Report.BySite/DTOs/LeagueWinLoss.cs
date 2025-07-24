namespace Fanex.Athena.Report.BySite.DTOs
{
    using System.Collections.Generic;

    public class LeagueWinLoss : BaseMatchWinLoss
    {
        public int Index { get; set; }

        public string ProductGroup { get; set; }

        public int SportType { get; set; }

        public int LeagueId { get; set; }

        public int BetTypeId { get; set; }

        public int BetCount { get; set; }

        public decimal TurnOver { get; set; }

        public decimal CustomerWL { get; set; }

        public decimal LicenseeWL { get; set; }

        public int Currency { get; set; }

        public string SportName { get; set; }

        public string LeagueName { get; set; }

        public string MatchName { get; set; }

        public string BetTypeName { get; set; }

        public bool IsBreakdown { get; set; }

        public bool IsDirectToCurrency { get; set; }

        public int PlayerCount { get; set; }

        public IEnumerable<LeagueWinLoss> Detail { get; set; }
    }
}