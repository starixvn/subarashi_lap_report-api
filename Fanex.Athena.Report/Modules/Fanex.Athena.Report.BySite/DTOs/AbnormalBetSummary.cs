namespace Fanex.Athena.Report.BySite.DTOs
{
    public class AbnormalBetSummary
    {
        public string Date { get; set; }

        public string Sport { get; set; }

        public string BetType { get; set; }

        public string Choice { get; set; }

        public string League { get; set; }

        public string Match { get; set; }

        public string KOTime { get; set; }

        public string Live { get; set; }

        public string FinalScore { get; set; }

        public string Reason { get; set; }

        public int TotalTransaction { get; set; }
    }
}