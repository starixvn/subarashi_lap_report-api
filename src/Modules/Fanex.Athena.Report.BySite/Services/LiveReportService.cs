namespace Fanex.Athena.Report.BySite.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Models;
    using Repositories.Interfaces;
    using Resources;
    using Resources.Constants;

    public class LiveReportService : ILiveReportService
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly ILiveReportRepository _liveReportRepository;

        public LiveReportService(ICacheRefAdapter cacheRefAdapter, ILiveReportRepository liveReportRepository)
        {
            _cacheRefAdapter = cacheRefAdapter;
            _liveReportRepository = liveReportRepository;
        }

        public async Task<IList<LiveReport>> GetLiveReportSummary(LiveReportMetaModel model) =>
            model.IsValid ? await _liveReportRepository.GetLiveReportSummary(model).ConfigureAwait(false) : new List<LiveReport>();

        public async Task<LiveReportDetailResultModel> GetLiveReportSummaryDetail(
            LiveReportMetaModel model,
            IDictionary<int, string> currencyList,
            IEnumerable<ProductInfo> products,
            int[] liveReportNoDetailLinkSports
        )
        {
            if (!model.IsValid)
            {
                return new LiveReportDetailResultModel
                {
                    DataBySport = new List<LiveReportBySport>(),
                    DataByCurrency = new List<LiveReport>(),
                    DataByBetType = new List<LiveReportByBetType>()
                };
            }

            var detailBySport = await _liveReportRepository.GetDetailBySport(model, liveReportNoDetailLinkSports).ConfigureAwait(false);

            var tupleCurrencyAndBettype = await _liveReportRepository
                .GetDetailByCurrencyAndBettype(model, currencyList, products)
                .ConfigureAwait(false);
            return new LiveReportDetailResultModel
            {
                DataBySport = detailBySport,
                DataByCurrency = tupleCurrencyAndBettype.Item1,
                DataByBetType = tupleCurrencyAndBettype.Item2
            };
        }

        public async Task<SortableList<LiveReportWinLossByMatch>> GetLiveReportWinLossByMatch(
            LiveReportMatchListMetalModel model)
        {
            if (!model.IsValid)
            {
                return new SortableList<LiveReportWinLossByMatch>();
            }

            var dataList = await _liveReportRepository.GetLiveReportWinlossByMatch(model).ConfigureAwait(false);

            foreach (var data in dataList)
            {
                data.MatchName = GetMatchName(model, data);
            }

            if (!string.IsNullOrEmpty(model.SortColumn) && !string.IsNullOrEmpty(model.SortDirection))
            {
                dataList.Sort(model.SortColumn, model.SortDirection.Equals("ASC", StringComparison.OrdinalIgnoreCase));
            }

            return dataList;
        }

        private string GetMatchName(LiveReportMatchListMetalModel model, LiveReportWinLossByMatch data)
        {
            string matchName;

            if (model.SportId == SportType.LiveCasino)
            {
                matchName = string.Format("{0} {1} {2} ", _cacheRefAdapter.GetSportName(model.SportId), _cacheRefAdapter.GetBettype(data.BetType),
                    data.ShowTime);
            }
            else
            {
                if (SportHelper.IsVSNewRacingSportTypes(model.SportId))
                {
                    var id = !string.IsNullOrEmpty(data.Distance) ? $" - ID {data.Distance}" : string.Empty;
                    matchName = $"{ViewResources.Event} {data.VendorMatchId}{id}";
                }
                else
                {
                    matchName = string.Format("{0} vs {1}", _cacheRefAdapter.GetTeamName(data.HomeId, data.SportId),
                        _cacheRefAdapter.GetTeamName(data.AwayId, data.SportId));
                }
            }

            return matchName;
        }
    }
}