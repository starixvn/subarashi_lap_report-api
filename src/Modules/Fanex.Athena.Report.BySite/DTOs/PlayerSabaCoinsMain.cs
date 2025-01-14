namespace Fanex.Athena.Report.BySite.DTOs
{
    public class PlayerSabaCoinsMain
    {
        public int Index { get; set; }

        public long SabaId { get; set; }

        public int CustId { get; set; }

        public string Username { get; set; }

        public string ExternalId { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal Turnover { get; set; }

        public decimal Outstanding { get; set; }

        public decimal WinLoss { get; set; }

        public decimal ClosingBalance { get; set; }

        public decimal RewardPromotionAmt { get; set; }

        public decimal ExchangeAmt { get; set; }
    }
}