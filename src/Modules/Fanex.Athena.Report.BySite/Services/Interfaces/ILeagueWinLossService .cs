namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface ILeagueWinLossService
    {
        Task<SortableReportResultModel<LeagueWinLoss>> GetDataAsync(LeagueWinLossMetaModel metaModel);

        Task<SortableReportResultModel<LeagueWinLoss>> GetCurrencyDataAsync(LeagueWinLossMetaModel metaModel);
    }
}