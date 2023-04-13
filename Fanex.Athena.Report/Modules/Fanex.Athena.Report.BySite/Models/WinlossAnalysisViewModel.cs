namespace Fanex.Athena.Report.BySite.Models
{
    public class WinlossAnalysisViewModel
    {
        public int RowIndex { get; set; }

        public string GroupBy { get; set; }

        public string GroupByFieldText { get; set; }

        public string BreakBy { get; set; }

        public string BreakDownFieldText { get; set; }

        public int BetCount { get; set; }

        public decimal Turnover { get; set; }

        public decimal NetTurnover { get; set; }

        public decimal PlayerWinloss { get; set; }

        public decimal PlayerComm { get; set; }

        public decimal LicWinloss { get; set; }

        public decimal LicComm { get; set; }

        public decimal AmtDueToBA { get; set; }

        public decimal BetEffective { get; set; }

        public float Margin { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }
    }
}
