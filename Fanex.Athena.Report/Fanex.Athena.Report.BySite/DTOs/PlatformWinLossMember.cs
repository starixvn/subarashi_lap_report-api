namespace Fanex.Athena.Report.BySite.DTOs
{
    public class PlatformWinLossMember
    {
        public PlatformWinLossMember()
        {
            CurrencyName = string.Empty;
        }

        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public decimal Turnover { get; set; }

        public int TotalTransactions { get; set; }

        public decimal LicenseeWL { get; set; }

        public decimal BACommission { get; set; }

        public decimal AmtDueToBA { get; set; }

        public int Index { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public int CustomerId { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal CustomerTotal
        {
            get { return CustomerWinLoss + CustomerComm; }
        }
    }
}
