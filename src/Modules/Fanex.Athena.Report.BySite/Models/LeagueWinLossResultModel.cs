namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class LeagueWinLossResultModel
    {
        public List<LeagueWinLoss> Data { get; set; }

        public LeagueWinLoss Total { get; set; }
    }
}