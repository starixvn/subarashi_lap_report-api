using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;

namespace Fanex.Athena.Report.BySite.Models
{
    public class LiveReportSummaryResultModel
    {
       public IList<LiveReport> Summary { get; set; }
    }
}
