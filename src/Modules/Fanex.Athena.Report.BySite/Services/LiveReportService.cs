using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services
{
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
            model.IsValid ?
                await _liveReportRepository.GetLiveReportSummary(model).ConfigureAwait(false) :
                new List<LiveReport>();

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

            switch (model.SportId)
            {
                case SportType.LiveCasino:
                    matchName = string.Format("{0} {1} {2} ", _cacheRefAdapter.GetSportName(model.SportId), _cacheRefAdapter.GetBettype(data.BetType), data.ShowTime);
                    break;

                default:
                    if (SportHelper.IsVSNewRacingSportTypes(model.SportId))
                    {
                        var id = !string.IsNullOrEmpty(data.Distance) ? $" - ID {data.Distance}" : string.Empty;
                        matchName = $"{ViewResources.Event} {data.VendorMatchId}{id}";
                    }
                    else
                    {
                        matchName = string.Format("{0} vs {1}", _cacheRefAdapter.GetTeamName(data.HomeId, data.SportId), _cacheRefAdapter.GetTeamName(data.AwayId, data.SportId));
                    }
                    break;
            }

            return matchName;
        }
    }
}