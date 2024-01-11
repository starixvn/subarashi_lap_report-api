namespace Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis
{
    public record SumTotalWinlossAnalysisDTO : WinlossAnalysisDTO
    {
        public int TotalRow { get; set; }
    }
}