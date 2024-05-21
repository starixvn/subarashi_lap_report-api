namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

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