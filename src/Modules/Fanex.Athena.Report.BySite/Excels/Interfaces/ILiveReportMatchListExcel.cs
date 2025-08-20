namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Extensions;
    using DTOs;
    using MetaModels;
    using Options;

    public interface ILiveReportMatchListExcel
    {
        byte[] Export(string template, SortableList<LiveReportWinLossByMatch> data,
                      LiveReportMatchListMetalModel metaModel,
                      LiveReportMatchListExcelOption excelOption);
    }
}
