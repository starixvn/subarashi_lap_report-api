using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis
{
    public class WinlossAnalysisAggregateDTO
    {
        public IEnumerable<WinlossAnalysisDTO> MainData { get; set; }

        public IEnumerable<WinlossAnalysisDTO> GroupSumData { get; set; }

        public SumTotalWinlossAnalysisDTO SumTotal { get; set; }

        public int TotalRow => SumTotal != null ? SumTotal.TotalRow : 0;
    }
}