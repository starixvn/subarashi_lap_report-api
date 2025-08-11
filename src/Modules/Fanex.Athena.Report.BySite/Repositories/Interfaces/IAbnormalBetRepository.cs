namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IAbnormalBetRepository
    {
        Task<SortableReportResultModel<AbnormalBetSummary>> GetSummaryData(
            AbnormalBetMetaModel metaModel);

        Task<SortableReportResultModel<AbnormalBetIpSummary>> GetIpSummaryData(
            AbnormalBetMetaModel metaModel);

        Task<SortableReportResultModel<AbnormalBetDetail>> GetDetailData(
            AbnormalBetMetaModel metaModel);
    }
}