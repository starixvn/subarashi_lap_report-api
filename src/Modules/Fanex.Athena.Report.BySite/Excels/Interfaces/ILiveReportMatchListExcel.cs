using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Extensions;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface ILiveReportMatchListExcel
    {
        byte[] Export(string template, SortableList<LiveReportWinLossByMatch> data,
                      LiveReportMatchListMetalModel metaModel,
                      LiveReportExcelOption excelOption);
    }
}
