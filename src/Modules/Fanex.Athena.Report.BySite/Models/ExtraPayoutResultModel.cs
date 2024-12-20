using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class ExtraPayoutResultModel
    {
        public List<ExtraPayout> Data { get; set; }

        public List<ExtraPayout> TotalCurrency { get; set; }

        public ExtraPayout TotalSummary { get; set; }

        public int TotalRows { get; set; }
    }
}