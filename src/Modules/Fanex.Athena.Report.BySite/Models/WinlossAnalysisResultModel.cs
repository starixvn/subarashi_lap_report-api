namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;
    using DTOs.WinlossAnalysis;

    public class WinlossAnalysisResultModel
    {
        public int TotalRow { get; set; }

        public IEnumerable<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>> Data { get; set; }

        public WinlossAnalysisDTO SumTotal { get; set; }
    }
}