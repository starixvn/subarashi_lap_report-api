using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class LeagueWinLossResultModel
    {
        public List<LeagueWinLoss> Data { get; set; }

        public LeagueWinLoss Total { get; set; }
    }
}