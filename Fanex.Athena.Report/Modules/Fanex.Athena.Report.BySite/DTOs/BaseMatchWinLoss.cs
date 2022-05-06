namespace Fanex.Athena.Report.BySite.DTOs
{
    public class BaseMatchWinLoss
    {
        public int MatchId { get; set; }

        public int HomeId { get; set; }

        public int AwayId { get; set; }

        public bool IsNeutral { get; set; }

        public int EventType { get; set; }
    }
}