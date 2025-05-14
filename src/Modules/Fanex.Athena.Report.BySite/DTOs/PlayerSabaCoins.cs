using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class PlayerSabaCoins
    {
        public long SabaId { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal Turnover { get; set; }

        public decimal Outstanding { get; set; }

        public decimal WinLoss { get; set; }

        public decimal ClosingBalance { get; set; }

        public decimal RewardPromotionAmt { get; set; }

        public decimal ExchangeAmt { get; set; }

        public IEnumerable<PlayerSabaCoinsAccount> Accounts { get; set; } = new List<PlayerSabaCoinsAccount>();
    }
}