using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;

namespace Fanex.Athena.Report.BySite.Models
{
    public class WinlossAnalysisResultModel
    {
        public int TotalRow { get; set; }

        public IEnumerable<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>> Data { get; set; }

        public WinlossAnalysisDTO SumTotal { get; set; }
    }
}