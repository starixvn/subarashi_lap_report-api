namespace Fanex.Athena.Report.BySite.Models
{
    public class SettledMatchCountResultModel
    {
        public string SportName { get; set; }

        public int YesterdayEntered { get; set; }

        public int YesterdayNonEntered { get; set; }

        public int TodayEntered { get; set; }

        public int TodayNonEntered { get; set; }
    }
}
