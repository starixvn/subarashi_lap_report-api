namespace Fanex.Athena.Report.BySite.DTOs
{
    public class LiveReportWinLossByMatch
    {
        public long MatchId { get; set; }

        public int HomeId { get; set; }

        public int AwayId { get; set; }

        public int BetType { get; set; }

        public string ShowTime { get; set; }

        public int SportId { get; set; }

        public string MatchName { get; set; }

        public decimal TurnOver { get; set; }

        public int BetCount { get; set; }

        public int EventType { get; set; }

        public string VendorMatchId { get; set; }

        public string Distance { get; set; }

        public decimal LicenseeComm { get; set; }

        public decimal AmountDue { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal CustomerTotal => CustomerComm + CustomerWinLoss;

        public decimal LicenseeWinLoss { get; set; }
    }
}
