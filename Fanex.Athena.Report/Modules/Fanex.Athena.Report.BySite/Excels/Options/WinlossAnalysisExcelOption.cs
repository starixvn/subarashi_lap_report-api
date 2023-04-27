using Fanex.Athena.Report.BySite.Enums;
using System;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public record WinlossAnalysisExcelOption
    {
        public string CurrencyList { get; set; }

        public string BaseCurrency { get; set; }

        public string ProductCodes { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public WinlossAnalysisExportMode ExportExcelMode { get; set; }

        public bool IsHasBAProduct { get; set; }

        public GroupByMode GroupByMode { get; set; }

        public BreakdownMode BreakdownMode { get; set; }
    }
}
