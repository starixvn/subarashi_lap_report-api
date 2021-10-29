namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface ILeagueWinLossRepository
    {
        Task<SortableReportResultModel<LeagueWinLoss>> GetLeagueWinlossDetail(
            LeagueWinLossMetaModel metaModel);

        Task<SortableReportResultModel<LeagueWinLoss>> GetMatchWinlossDetail(
            LeagueWinLossMetaModel metaModel);

        Task<SortableReportResultModel<LeagueWinLoss>> GetBettypeWinlossDetail(
            LeagueWinLossMetaModel metaModel);

        Task<SortableReportResultModel<LeagueWinLoss>> GetLeagueWinlossCurrencyDetail(
            LeagueWinLossMetaModel metaModel);

        Task<SortableReportResultModel<LeagueWinLoss>> GetMatchWinlossCurrencyDetail(
            LeagueWinLossMetaModel metaModel);

        Task<SortableReportResultModel<LeagueWinLoss>> GetBettypeWinlossCurrencyDetail(
            LeagueWinLossMetaModel metaModel);
    }
}