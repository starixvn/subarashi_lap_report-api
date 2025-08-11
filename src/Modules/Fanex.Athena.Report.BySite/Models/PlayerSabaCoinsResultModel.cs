namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class PlayerSabaCoinsResultModel
    {
        public List<PlayerSabaCoins> Data { get; set; }

        public PlayerSabaCoins TotalSummary { get; set; }

        public int TotalRows { get; set; }
    }
}