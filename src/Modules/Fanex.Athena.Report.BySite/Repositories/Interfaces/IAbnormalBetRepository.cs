namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Threading.Tasks;

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