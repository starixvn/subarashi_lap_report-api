namespace Fanex.Athena.Report.ByPlayer.DTO
{
    public class CustomerBalance
    {
        public int Index { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal Deposit { get; set; }

        public decimal Withdrawal { get; set; }

        public decimal Bonus { get; set; }

        public decimal Adjustment { get; set; }

        public decimal Turnover { get; set; }

        public decimal DrawGame { get; set; }

        public decimal NetTurnover { get; set; }

        public decimal PendingFunds { get; set; }

        public decimal WinLoss { get; set; }

        public decimal PlayerComm { get; set; }

        public decimal TotalWL { get; set; }

        public decimal ClosingBalance { get; set; }

        public decimal AffiliateTransfer { get; set; }

        public decimal TurnoverBA { get; set; }

        public decimal CommBA { get; set; }

        public decimal AmountDueToBA { get; set; }

        public decimal Promotion { get; set; }
    }
}