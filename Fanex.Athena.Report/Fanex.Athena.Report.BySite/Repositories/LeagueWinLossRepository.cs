namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Data;

    public class LeagueWinLossRepository : ILeagueWinLossRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private const int Parlay = 0;
        private const int Outright = 2;
        private const string BetTypeListColumn = "betTypeList";
        private const string BalanceUpDayColumn = "balanceupday";
        private const string MatchIdColumn = "matchId";
        private const string EventTypeColumn = "eventType";

        private static readonly List<Tuple<int, int, string>> SabaVirtualSports = new List<Tuple<int, int, string>>
        {
            Tuple.Create( SportType.SabaSoccer, SportType.Soccer, "" ),
            Tuple.Create( SportType.SabaSoccerPingoal, 0, ViewResources.SoccerPinGoal),
            Tuple.Create( SportType.SabaBasketball, SportType.BasketBall, "" ),
            Tuple.Create( SportType.SabaBasketballPinGoal, 0, ViewResources.BasketballPinGoal),
            Tuple.Create( SportType.SabaCricket, SportType.Cricket, "" ),
            Tuple.Create( SportType.SabaFutsal, SportType.Futsal, "" )
        };

        public LeagueWinLossRepository(
            IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetLeagueWinlossDetail(
            LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_League";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("sportTypeList", metaModel.SportIdList);
                db.SetParameterValue("leagueName", metaModel.LeagueName);
                db.SetParameterValue("searchLanguage", Thread.CurrentThread.CurrentUICulture.Name);
                db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay, true);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetLeagueData(reader);
                }
            }
        }

        private static void ReadCommonData(IDataReader reader, LeagueWinLoss data)
        {
            data.CustomerWL = reader.GetVal<decimal>("winlost");
            data.BetCount = reader.GetVal<int>("betcount");
            data.LicenseeWL = reader.GetVal<decimal>("LicenseeWL");
            data.TurnOver = reader.GetVal<decimal>("turnover");
            data.BAComm = reader.GetVal<decimal>("BAComm");
        }

        private static LeagueWinLoss GetSumData(IEnumerable<LeagueWinLoss> detailData)
        {
            var sumData = new LeagueWinLoss
            {
                BetCount = detailData.Sum(_ => _.BetCount),
                TurnOver = detailData.Sum(_ => _.TurnOver),
                CustomerWL = detailData.Sum(_ => _.CustomerWL),
                LicenseeWL = detailData.Sum(_ => _.LicenseeWL),
                BAComm = detailData.Sum(_ => _.BAComm),
                IsBreakdown = true
            };

            return sumData;
        }

        private void BuildSportTotalData(List<LeagueWinLoss> data, SortableList<LeagueWinLoss> dataList)
        {
            var sportList = data.Where(_ => _.SportType >= 0).Select(_ => _.SportType).Distinct().OrderBy(_ => _).ToList();
            sportList.AddRange(data.Where(_ => _.SportType < 0).OrderByDescending(_ => _.SportType).Select(_ => _.SportType).Distinct().ToList());
            var index = 1;

            foreach (var sport in sportList)
            {
                var sportDetail = data.Where(_ => _.SportType == sport);
                var sportData = GetSumData(sportDetail);

                sportData.SportType = sport;
                sportData.SportName = GetSportName(sport);
                sportData.Index = index++;
                sportData.Detail = sportDetail;
                if (sport == Parlay)
                {
                    sportData.IsDirectToCurrency = true;
                    sportData.IsBreakdown = false;
                    sportData.LeagueId = sportDetail.FirstOrDefault().LeagueId;
                }
                dataList.Add(sportData);
            }
        }

        private string GetSportName(int sport)
        {
            if (sport == Parlay)
            {
                return ViewResources.Parlay;
            }
            else if (sport > 0)
            {
                return _cacheRefAdapter.GetSportName(sport);
            }
            else
            {
                var sabaSport = sport < 0 ? SabaVirtualSports.Find(_ => _.Item1 == sport).Item3 : _cacheRefAdapter.GetSportName(SabaVirtualSports.Find(_ => _.Item1 == sport).Item2);
                return ViewResources.Saba + (sabaSport);
            }
        }

        private static void UpdateSpCommonParamsAsync(IObjectDb db, LeagueWinLossMetaModel metaModel, DateTime moveBetsDay, bool applyBaseCurrency = false)
        {
            db.SetParameterValue("Username", metaModel.UserName);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("productList", metaModel.ProductCodeList);
            db.SetParameterValue("showBA", metaModel.ShowBA ? 1 : 0);
            db.SetParameterValue("statusIdList", metaModel.StatusIdList);
            db.SetParameterValue("movebetsday", moveBetsDay);
            db.SetParameterValue("BaseCurrency", !metaModel.IsGroupInOriginalCurrency || applyBaseCurrency ? metaModel.BaseCurrency.ToUpperInvariant() : string.Empty);
        }

        private string GetLeagueName(int leagueId, int sportType = 0)
        {
            var leagueName = sportType == SportType.BitCoin ? _cacheRefAdapter.GetLeagueName3rd(leagueId) : _cacheRefAdapter.GetLeagueName(leagueId);
            return leagueName;
        }

        private SortableReportResultModel<LeagueWinLoss> GetLeagueData(
            IDataReader reader)
        {
            var dataList = new SortableList<LeagueWinLoss>();
            var data = new List<LeagueWinLoss>();

            while (reader.Read())
            {
                var leagueId = reader.GetVal<int>("LeagueId");
                var sportType = reader.GetVal<int>("SportType");

                var item = new LeagueWinLoss
                {
                    SportType = sportType,
                    LeagueId = leagueId,
                    LeagueName = GetLeagueName(leagueId, sportType)
                };
                ReadCommonData(reader, item);

                data.Add(item);
            }

            BuildSportTotalData(data, dataList);

            return new SortableReportResultModel<LeagueWinLoss>
            {
                DataList = dataList,
                TotalData = GetSumData(data)
            };
        }

        private SortableReportResultModel<LeagueWinLoss> GetCurrencyData(
            IDataReader reader)
        {
            var dataList = new SortableList<LeagueWinLoss>();

            while (reader.Read())
            {
                var item = new LeagueWinLoss
                {
                    Currency = reader.GetVal<int>("Currency")
                };
                ReadCommonData(reader, item);

                dataList.Add(item);
            }

            return new SortableReportResultModel<LeagueWinLoss>
            {
                DataList = dataList
            };
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetMatchWinlossDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_Match";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("leagueId", metaModel.LeagueId);
                db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
                db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay, true);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetMatchData(reader, metaModel.LeagueId);
                }
            }
        }

        private SortableReportResultModel<LeagueWinLoss> GetMatchData(
            IDataReader reader, int leagueId)
        {
            var dataList = new SortableList<LeagueWinLoss>();
            var data = new List<LeagueWinLoss>();

            while (reader.Read())
            {
                var matchId = reader.GetVal<int>("MatchId");
                var homeId = reader.GetVal<int>("HomeId");
                var awayId = reader.GetVal<int>("AwayId");
                var isNeutral = reader.GetVal<bool>("IsNeutral");
                var eventType = reader.GetVal<int>("EventType");
                string matchName = eventType == Outright ? _cacheRefAdapter.GetTeamName(homeId) : _cacheRefAdapter.GetMatchName(homeId, awayId, isNeutral);

                var item = new LeagueWinLoss
                {
                    MatchId = matchId,
                    MatchName = matchName,
                    EventType = eventType,
                    HomeId = homeId,
                    AwayId = awayId
                };
                ReadCommonData(reader, item);

                data.Add(item);
            }

            BuildLeagueTotalData(data, dataList, leagueId);

            return new SortableReportResultModel<LeagueWinLoss>
            {
                DataList = dataList,
                TotalData = GetSumData(data)
            };
        }

        private void BuildLeagueTotalData(List<LeagueWinLoss> data, SortableList<LeagueWinLoss> dataList, int leagueId)
        {
            var leagueData = GetSumData(data);
            leagueData.Detail = data.OrderBy(_ => _.EventType).ThenBy(_ => _.MatchId);
            leagueData.LeagueName = GetLeagueName(leagueId);
            dataList.Add(leagueData);
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetBettypeWinlossDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_BetType";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue(MatchIdColumn, metaModel.MatchId);
                db.SetParameterValue(EventTypeColumn, metaModel.EventType);
                db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
                db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay, true);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetBetTypeData(reader, metaModel);
                }
            }
        }

        private SortableReportResultModel<LeagueWinLoss> GetBetTypeData(
            IDataReader reader, LeagueWinLossMetaModel metaModel)
        {
            var dataList = new SortableList<LeagueWinLoss>();
            var data = new List<LeagueWinLoss>();

            while (reader.Read())
            {
                var betTypeId = reader.GetVal<int>("BetType");

                var item = new LeagueWinLoss
                {
                    BetTypeId = betTypeId,
                    BetTypeName = _cacheRefAdapter.GetBettype(betTypeId),
                    MatchId = metaModel.MatchId,
                    EventType = metaModel.EventType
                };
                ReadCommonData(reader, item);

                data.Add(item);
            }

            BuildMatchTotalData(data, dataList, metaModel);

            return new SortableReportResultModel<LeagueWinLoss>
            {
                DataList = dataList,
                TotalData = GetSumData(data)
            };
        }

        private static void BuildMatchTotalData(List<LeagueWinLoss> data, SortableList<LeagueWinLoss> dataList, LeagueWinLossMetaModel metaModel)
        {
            var matchData = GetSumData(data);
            matchData.Detail = data;
            matchData.MatchName = metaModel.MatchName;
            dataList.Add(matchData);
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetLeagueWinlossCurrencyDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_League_Currency";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("leagueId", metaModel.LeagueId);
                db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetCurrencyData(reader);
                }
            }
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetMatchWinlossCurrencyDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_Match_Currency";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue(MatchIdColumn, metaModel.MatchId);
                db.SetParameterValue(EventTypeColumn, metaModel.EventType);
                db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
                db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetCurrencyData(reader);
                }
            }
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetBettypeWinlossCurrencyDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_BetType_Currency";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue(MatchIdColumn, metaModel.MatchId);
                db.SetParameterValue(EventTypeColumn, metaModel.EventType);
                db.SetParameterValue("betType", metaModel.BettypeId);
                db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetCurrencyData(reader);
                }
            }
        }
    }
}