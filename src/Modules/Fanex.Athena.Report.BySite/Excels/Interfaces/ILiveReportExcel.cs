using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface ILiveReportExcel
    {
        byte[] Export(string template, IList<LiveReport> summaryLiveReports, LiveReportDetailResultModel data, LiveReportMetaModel metaModel,
                      LiveReportExcelOption excelOption);
    }
}
