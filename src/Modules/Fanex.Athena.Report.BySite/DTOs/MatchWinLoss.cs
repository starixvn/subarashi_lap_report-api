using System;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class MatchWinLoss
    {
        public DateTime KickOffTime { get; set; }

        public int LeagueId { get; set; }

        public int LeagueGroupId { get; set; }

        public string LeagueName { get; set; }

        public short SportType { get; set; }

        public int MatchId { get; set; }

        public string MatchName { get; set; }

        public string MatchCode { get; set; }

        public string HomeName { get; set; }

        public string AwayName { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public string VendorMatchId { get; set; }

        public string Distance { get; set; }

        public bool IsLive { get; set; }

        public int HomeId { get; set; }

        public int AwayId { get; set; }

        public int BetType { get; set; }

        public int EventType { get; set; }

        public string BettypeName { get; set; }

        public bool IsNeutral { get; set; }

        public decimal Turnover { get; set; }

        public decimal TotalTransaction { get; set; }

        public decimal CustomerWL { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal LicenseeWL { get; set; }
    }
}