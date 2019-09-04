namespace Fanex.Athena.Report.BySite.DTOs
{
    public class PlatformWinLossSummary
    {
        public string Name { get; set; }

        public string Parent { get; set; }

        public byte Level { get; set; }

        public int PlatformType { get; set; }

        public bool IsLeaf { get; set; }

        public decimal Turnover { get; set; }

        public decimal TurnoverPercent { get; set; }

        public int TotalTransactions { get; set; }

        public decimal LicenseeWL { get; set; }

        public string ProductCode { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal CustomerTotal
        {
            get { return CustomerWinLoss + CustomerComm; }
        }
    }
}
