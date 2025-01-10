namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Threading.Tasks;

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