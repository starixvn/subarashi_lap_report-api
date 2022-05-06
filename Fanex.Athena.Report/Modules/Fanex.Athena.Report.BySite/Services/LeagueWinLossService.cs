namespace Fanex.Athena.Report.BySite.Services
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources.Constants;

    public class LeagueWinLossService : ILeagueWinLossService
    {
        private readonly ILeagueWinLossRepository _repository;

        public LeagueWinLossService(ILeagueWinLossRepository repository)
        {
            _repository = repository;
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetDataAsync(LeagueWinLossMetaModel metaModel)
        {
            SortableReportResultModel<LeagueWinLoss> result;

            if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.League)
            {
                result = await _repository.GetLeagueWinlossDetail(metaModel).ConfigureAwait(false);
            }
            else if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.Match)
            {
                result = await _repository.GetMatchWinlossDetail(metaModel).ConfigureAwait(false);
            }
            else
            {
                result = await _repository.GetBettypeWinlossDetail(metaModel).ConfigureAwait(false);
            }

            return result;
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetCurrencyDataAsync(LeagueWinLossMetaModel metaModel)
        {
            SortableReportResultModel<LeagueWinLoss> result;

            if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.League)
            {
                result = await _repository.GetLeagueWinlossCurrencyDetail(metaModel).ConfigureAwait(false);
            }
            else if (metaModel.DrillDownLevel == (int)LeagueWinLossReportLevel.Match)
            {
                result = await _repository.GetMatchWinlossCurrencyDetail(metaModel).ConfigureAwait(false);
            }
            else
            {
                result = await _repository.GetBettypeWinlossCurrencyDetail(metaModel).ConfigureAwait(false);
            }

            return result;
        }
    }
}