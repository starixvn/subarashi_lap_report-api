namespace Fanex.Athena.Report.BySite.Services
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Services.Interfaces;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Repositories.Interfaces;
    using Resources;
    using Resources.Constants;

    public class MatchWinLossService : IMatchWinLossService
    {
        private const string KickOffTimeFormat = "{0:MM/dd/yyyy hh:mm:ss tt}";

        private static readonly CultureInfo EnglishCulture = new("en-US");

        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly IMatchWinLossRepository _matchWinLossRepository;

        public MatchWinLossService(IMatchWinLossRepository matchWinLossRepository, ICacheRefAdapter cacheRefAdapter)
        {
            _matchWinLossRepository = matchWinLossRepository;
            _cacheRefAdapter = cacheRefAdapter;
        }

        #region IMatchWinLossService Members

        public async Task<IList<MatchWinLoss>> GetDataList(MatchWinLossMetaModel metaModel)
        {
            var dataList = await _matchWinLossRepository.GetDataList(metaModel).ConfigureAwait(false);

            foreach (var data in dataList)
            {
                data.LeagueName = GetLeagueName(data);
                data.MatchName = GetMatchName(data);
                data.HomeName = GetHomeName(data);
                data.AwayName = GetAwayName(data);
            }

            return OrderByProduct(dataList);
        }

        public async Task<DataSet> GetDetailDataSet(MatchWinLossMetaModel metaModel) =>
            await _matchWinLossRepository.GetDetailDataSet(metaModel).ConfigureAwait(false);

        public async Task<IList<MatchWinLoss>> GetDetailDataList(MatchWinLossMetaModel metaModel) =>
            await _matchWinLossRepository.GetDetailDataList(metaModel).ConfigureAwait(false);

        public async Task<IList<MatchWinLoss>> GetOutrightParlayList(MatchWinLossMetaModel metaModel) =>
            await _matchWinLossRepository.GetOutrightParlayList(metaModel).ConfigureAwait(false);

        #endregion IMatchWinLossService Members

        #region Utility

        private static IList<MatchWinLoss> OrderByProduct(IList<MatchWinLoss> dataList)
        {
            var result = dataList.Where(p => p.ProductName == Product.SB || p.ProductName == Product.BA)
                                 .GroupBy(p => p.MatchId)
                                 .Select(g => new MatchWinLoss
                                 {
                                     LeagueId = g.Select(r => r.LeagueId).FirstOrDefault(),
                                     LeagueName = g.Select(r => r.LeagueName).FirstOrDefault(),
                                     SportType = g.Select(r => r.SportType).FirstOrDefault(),
                                     MatchId = g.Select(r => r.MatchId).FirstOrDefault(),
                                     MatchName = g.Select(r => r.MatchName).FirstOrDefault(),
                                     MatchCode = g.Select(r => r.MatchCode).FirstOrDefault(),
                                     HomeName = g.Select(r => r.HomeName).FirstOrDefault(),
                                     AwayName = g.Select(r => r.AwayName).FirstOrDefault(),
                                     ProductName = Product.SB,
                                     IsLive = g.Select(r => r.IsLive).FirstOrDefault(),
                                     HomeId = g.Select(r => r.HomeId).FirstOrDefault(),
                                     AwayId = g.Select(r => r.AwayId).FirstOrDefault(),
                                     BetType = g.Select(r => r.BetType).FirstOrDefault(),
                                     IsNeutral = g.Select(r => r.IsNeutral).FirstOrDefault(),
                                     EventType = g.Select(r => r.EventType).FirstOrDefault(),
                                     Turnover = g.Sum(r => r.Turnover),
                                     TotalTransaction = g.Sum(r => r.TotalTransaction),
                                     CustomerWL = g.Sum(r => r.CustomerWL),
                                     CustomerComm = g.Sum(r => r.CustomerComm),
                                     LicenseeWL = g.Sum(r => r.LicenseeWL)
                                 })
                                 .OrderBy(m => m.LeagueName).ThenBy(m => m.MatchName).ToList();
            result.AddRange(
                dataList.Where(p => p.ProductName == Product.NG).OrderBy(m => m.LeagueName).ThenBy(m => m.MatchName));
            result.AddRange(
                dataList.Where(p => p.ProductName == Product.VS).OrderBy(m => m.LeagueName).ThenBy(m => m.MatchName));

            return result;
        }

        private string GetHomeName(MatchWinLoss matchWinLoss)
        {
            if (matchWinLoss.ProductName == Product.NG)
            {
                return string.Empty;
            }

            return _cacheRefAdapter.GetTeamName(matchWinLoss.HomeId, matchWinLoss.SportType) +
                   (matchWinLoss.IsNeutral ? " " + ViewResources.IsNeutral : string.Empty);
        }

        private string GetAwayName(MatchWinLoss matchWinLoss) =>
            matchWinLoss.ProductName == Product.NG ?
                string.Empty :
                _cacheRefAdapter.GetTeamName(matchWinLoss.AwayId, matchWinLoss.SportType);

        private string GetLeagueName(MatchWinLoss matchWinLoss)
            => _cacheRefAdapter.GetLeagueName(matchWinLoss.LeagueId, matchWinLoss.SportType);

        private string GetMatchName(MatchWinLoss matchWinLoss)
        {
            if (matchWinLoss.ProductName == Product.NG)
            {
                var prefix = matchWinLoss.SportType == SportType.NumberGameHappy5 ?
                    ViewResources.No + "." :
                    ViewResources.NumberGameNo;
                return prefix + " " + matchWinLoss.MatchCode;
            }

            if (SportHelper.IsVSSportTypes(matchWinLoss.SportType))
            {
                return string.Format(EnglishCulture, KickOffTimeFormat, matchWinLoss.KickOffTime);
            }

            if (SportHelper.IsVSNewRacingSportTypes(matchWinLoss.SportType))
            {
                var id = !string.IsNullOrEmpty(matchWinLoss.Distance) ? $" - ID {matchWinLoss.Distance}" : string.Empty;
                return $"{ViewResources.Event} {matchWinLoss.VendorMatchId}{id}";
            }

            var homeName = _cacheRefAdapter.GetTeamName(matchWinLoss.HomeId, matchWinLoss.SportType) +
                           (matchWinLoss.IsNeutral ? " " + ViewResources.IsNeutral : string.Empty);
            var awayName = _cacheRefAdapter.GetTeamName(matchWinLoss.AwayId, matchWinLoss.SportType);
            return homeName + "-vs-" + awayName;
        }

        #endregion Utility
    }
}