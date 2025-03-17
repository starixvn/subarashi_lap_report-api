namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class MatchInfo
    {
        public int SportType { get; set; }

        public int HomeId { get; set; }

        public int AwayId { get; set; }

        public string VendorMatchId { get; set; }

        public string Distance { get; set; }

        public DateTime KickOffTime { get; set; }

        public bool IsNeutral { get; set; }
    }
}