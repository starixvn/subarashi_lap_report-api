using Fanex.Data;

namespace Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis
{
    public record WinlossAnalysisDTO
    {
        public int RowIndex { get; set; }

        public string GroupBy { get; set; }

        public string BreakBy { get; set; }

        public int BetCount { get; set; }

        public decimal Turnover { get; set; }

        public decimal NetTurnover { get; set; }

        public decimal MemWinloss { get; set; }

        public decimal MemComm { get; set; }

        public decimal LicWinloss { get; set; }

        public decimal LicComm { get; set; }

        public decimal AmtDueToBA { get; set; }

        public decimal BetEffective { get; set; }

        public float Margin { get; set; }

        public decimal BaseTurnover { get; set; }

        public decimal BaseNetTurnover { get; set; }

        public decimal BaseMemWinloss { get; set; }

        public decimal BaseMemComm { get; set; }

        public decimal BaseLicWinloss { get; set; }

        public decimal BaseLicComm { get; set; }

        public decimal BaseAmtDueToBA { get; set; }

        public decimal BaseBetEffective { get; set; }

        public string SportName { get; set; }

        public string PlatformName { get; set; }

        public string ProductName { get; set; }

        public string BetTypeName { get; set; }

        public string ExternalId { get; set;}

        public string SystemId { get; set; }

        public string BreakDownText { get; set; }
    }
}
