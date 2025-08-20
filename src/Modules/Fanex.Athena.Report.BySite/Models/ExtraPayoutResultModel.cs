namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class ExtraPayoutResultModel
    {
        public List<ExtraPayout> Data { get; set; }

        public List<ExtraPayout> TotalCurrency { get; set; }

        public ExtraPayout TotalSummary { get; set; }

        public int TotalRows { get; set; }
    }
}