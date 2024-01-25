namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

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