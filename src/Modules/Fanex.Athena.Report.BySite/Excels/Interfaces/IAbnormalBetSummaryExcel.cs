namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IAbnormalBetSummaryExcel
    {
        byte[] Export(string template, SortableReportResultModel<AbnormalBetSummary> data, AbnormalBetMetaModel metaModel);
    }
}