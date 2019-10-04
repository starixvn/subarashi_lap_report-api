namespace Fanex.Athena.Report.ByPlayer.DTO
{
    public class CustomerWinLoss
    {
        public int Index { get; set; }

        public int CustId { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public decimal Turnover { get; set; }

        public int TotalTransaction { get; set; }

        public decimal CustomerWL { get; set; }

        public decimal PlayerComm { get; set; }

        public decimal CustomerTotalWL { get; set; }

        public decimal BetEffective { get; set; }

        public decimal LicenseeWL { get; set; }

        public decimal BAComm { get; set; }

        public decimal AmtDueToBA { get; set; }
    }
}