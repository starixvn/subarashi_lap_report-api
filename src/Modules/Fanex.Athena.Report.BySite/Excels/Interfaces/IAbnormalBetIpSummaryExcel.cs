namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IAbnormalBetIpSummaryExcel
    {
        byte[] Export(string template, SortableReportResultModel<AbnormalBetIpSummary> data, AbnormalBetMetaModel metaModel);
    }
}