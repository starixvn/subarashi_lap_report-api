namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IGodOfFortuneWinLossRepository
    {
        Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetAgentWinlossDetail(
            GodOfFortuneWinLossMetaModel metaModel);

        Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetCustomerWinlossDetail(
            GodOfFortuneWinLossMetaModel metaModel);

        Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetSummaryWinloss(
            GodOfFortuneWinLossMetaModel metaModel);
    }
}