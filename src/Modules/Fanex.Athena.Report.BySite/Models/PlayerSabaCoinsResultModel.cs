using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;

namespace Fanex.Athena.Report.BySite.Models
{
    public class PlayerSabaCoinsResultModel
    {
        public List<PlayerSabaCoins> Data { get; set; }

        public PlayerSabaCoins TotalSummary { get; set; }

        public int TotalRows { get; set; }
    }
}