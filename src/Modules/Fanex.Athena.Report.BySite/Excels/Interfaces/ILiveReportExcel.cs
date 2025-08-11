namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using System.Collections.Generic;
    using DTOs;
    using MetaModels;
    using Models;
    using Options;

    public interface ILiveReportExcel
    {
        byte[] Export(string template, IList<LiveReport> summaryLiveReports, LiveReportDetailResultModel data, LiveReportMetaModel metaModel,
                      LiveReportExcelOption excelOption);
    }
}
