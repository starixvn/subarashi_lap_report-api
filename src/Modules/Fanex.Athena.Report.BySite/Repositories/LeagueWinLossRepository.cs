namespace Fanex.Athena.Report.BySite.Repositories
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Helpers;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.Core.Services.Interfaces;

    public class LeagueWinLossRepository : ILeagueWinLossRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly INameService _nameService;
        private readonly IProductService _productService;

        private const int ParlaySport = 0;
        private const int Outright = 2;
        private const int ParlaySB = -9;
        private const int ParlayVS = -5;
        private const string BetTypeListColumn = "betTypeList";
        private const string BalanceUpDayColumn = "balanceupday";
        private const string MatchIdColumn = "matchId";
        private const string EventTypeColumn = "eventType";
        private const string KickOfTimeFormat = "{0:MM/dd/yyyy hh:mm:ss tt}";
        private static readonly CultureInfo EnglishCulture = new("en-US");

        private static readonly List<string> ProductsGroupOrder = new() { Product.SB, Product.VS };
        private static readonly List<int> SportsDirectToCurrencyOrder = new() { ParlaySport, SportType.SportsLottery, SportType.BitCoin };

        public LeagueWinLossRepository(
            IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter, INameService nameService, IProductService productService)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
            _nameService = nameService;
            _productService = productService;
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetLeagueWinlossDetail(
            LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_League";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("sportTypeList", metaModel.SportIdList);
            db.SetParameterValue("leagueName", metaModel.LeagueName);
            db.SetParameterValue("searchLanguage", CultureInfo.CurrentUICulture.Name);
            db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
            db.SetParameterValue("BalanceUpDay", balanceUpInfo.BalanceUpDay);
            UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay, true);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return GetLeagueData(reader, metaModel.IsHideSaba);
        }

        private static void ReadCommonData(IDataReader reader, LeagueWinLoss data)
        {
            data.CustomerWL = reader.GetVal<decimal>("winlost");
            data.BetCount = reader.GetVal<int>("betcount");
            data.LicenseeWL = reader.GetVal<decimal>("LicenseeWL");
            data.TurnOver = reader.GetVal<decimal>("turnover");
        }

        private static LeagueWinLoss GetSumData(IEnumerable<LeagueWinLoss> detailData)
        {
            var sumData = new LeagueWinLoss
            {
                BetCount = detailData.Sum(_ => _.BetCount),
                TurnOver = detailData.Sum(_ => _.TurnOver),
                CustomerWL = detailData.Sum(_ => _.CustomerWL),
                LicenseeWL = detailData.Sum(_ => _.LicenseeWL),
                IsBreakdown = true
            };

            return sumData;
        }

        private static List<(int SportType, string ProductGroup, int LeagueId)> SortSportList(
            IEnumerable<(int SportType, string ProductGroup, int LeagueId)> data)
        {
            const int LowestPriority = int.MaxValue;
            const int DefaultLeaguePriority = 1000;

            return data
                .Distinct()
                .OrderBy(_ => ProductsGroupOrder.IndexOf(_.ProductGroup))
                .ThenBy(_ => GetSportTypePriority(_.SportType))
                .ThenBy(_ => Math.Abs(_.SportType))
                .ThenBy(_ =>
                {
                    if (_.SportType == 0)
                    {
                        return (_.LeagueId == ParlaySB || _.LeagueId == ParlayVS) ? LowestPriority : DefaultLeaguePriority;
                    }
                    return 0;
                })
                .ToList();
        }

        private static int GetSportTypePriority(int sportType)
        {
            const int PositiveSportPriority = 1;
            const int LotteryPriority = 2;
            const int BitCoinPriority = 3;
            const int NegativeSportPriority = 4;
            const int ZeroSportPriority = 5;

            return sportType switch
            {
                SportType.SportsLottery => LotteryPriority,
                SportType.BitCoin => BitCoinPriority,
                > 0 => PositiveSportPriority,
                < 0 => NegativeSportPriority,
                _ => ZeroSportPriority
            };
        }

        private void BuildSportTotalData(List<LeagueWinLoss> data, SortableList<LeagueWinLoss> dataList, bool isHideSaba)
        {
            var sportList = SortSportList(data.Select(_ => (_.SportType, _.ProductGroup, SportsDirectToCurrencyOrder.Contains(_.SportType) ? _.LeagueId : 0)));

            var index = 1;

            foreach (var sport in sportList)
            {
                var sportDetail = data.Where(
                    _ => _.SportType == sport.SportType
                    && _.ProductGroup == sport.ProductGroup
                    && (sport.LeagueId == 0 || _.LeagueId == sport.LeagueId));

                var sportData = GetSumData(sportDetail);
                var sportName = GetSportName(sport.SportType, sport.LeagueId, isHideSaba);
                var productName = _productService.GetProductName(sport.ProductGroup);

                sportData.ProductGroup = sport.ProductGroup;
                sportData.SportType = sport.SportType;
                sportData.SportName = String.IsNullOrEmpty(sportName) ? productName : $"{productName} > {sportName}";
                sportData.Index = index++;
                sportData.Detail = sportDetail;

                if (SportsDirectToCurrencyOrder.Contains(sport.SportType))
                {
                    sportData.IsDirectToCurrency = true;
                    sportData.IsBreakdown = false;
                    sportData.LeagueId = sport.LeagueId;
                }

                dataList.Add(sportData);
            }
        }

        private string GetSportName(int sport, int leagueId, bool isHideSaba)
        {
            if (sport == ParlaySport)
            {
                return GetProductName(leagueId);
            }

            return _nameService.GetSportName(sport, isHideSaba);
        }

        private static string GetProductName(int leagueId)
        {
            switch (leagueId)
            {
                case ParlaySB:
                case ParlayVS:
                    return ViewResources.Parlay;

                default:
                    return string.Empty;
            }
        }

        private static void UpdateSpCommonParamsAsync(IObjectDb db, LeagueWinLossMetaModel metaModel, DateTime moveBetsDay, bool applyBaseCurrency = false)
        {
            db.SetParameterValue("Username", metaModel.UserName);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("productList", metaModel.ProductCodeList);
            db.SetParameterValue("statusIdList", metaModel.StatusIdList);
            db.SetParameterValue("movebetsday", moveBetsDay);
            db.SetParameterValue("BaseCurrency", !metaModel.IsGroupInOriginalCurrency || applyBaseCurrency ? metaModel.BaseCurrency.ToUpperInvariant() : string.Empty);
        }

        private string GetLeagueName(int leagueId, bool isHideSaba, int sportType = 0)
        {
            var leagueName = _cacheRefAdapter.GetLeagueName(leagueId, sportType);
            if (isHideSaba)
            {
                leagueName = System.Text.RegularExpressions.Regex.Replace(
                        leagueName, ViewResources.Saba
                        , string.Empty
                        , System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return leagueName;
        }

        private SortableReportResultModel<LeagueWinLoss> GetLeagueData(
            IDataReader reader,
            bool isHideSaba)
        {
            var dataList = new SortableList<LeagueWinLoss>();
            var data = new List<LeagueWinLoss>();

            while (reader.Read())
            {
                var leagueId = reader.GetVal<int>("LeagueId");
                var sportType = reader.GetVal<int>("SportType");
                var productCode = reader.GetVal<string>("GroupProductCode");
                var leagueName = GetLeagueName(leagueId, isHideSaba, sportType);
                var productName = productCode;

                var item = new LeagueWinLoss
                {
                    ProductGroup = productName,
                    SportType = sportType,
                    LeagueId = leagueId,
                    LeagueName = leagueName
                };
                ReadCommonData(reader, item);

                data.Add(item);
            }

            BuildSportTotalData(data, dataList, isHideSaba);

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

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("leagueId", metaModel.LeagueId);
            db.SetParameterValue("SportType", metaModel.SportType);
            db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
            db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
            UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay, true);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return GetMatchData(reader, metaModel.LeagueId, metaModel.SportType, metaModel.IsHideSaba);
        }

        private SortableReportResultModel<LeagueWinLoss> GetMatchData(
            IDataReader reader, int leagueId, int sportType, bool isHideSaba)
        {
            var dataList = new SortableList<LeagueWinLoss>();
            var data = new List<LeagueWinLoss>();

            while (reader.Read())
            {
                var matchId = reader.GetVal<int>("MatchId");
                var eventType = reader.GetVal<int>("EventType");

                var matchInfo = new MatchInfo
                {
                    HomeId = reader.GetVal<int>("HomeId"),
                    AwayId = reader.GetVal<int>("AwayId"),
                    IsNeutral = reader.GetVal<bool>("IsNeutral"),
                    VendorMatchId = reader.GetVal<string>("VendorMatchId"),
                    Distance = reader.GetVal<string>("Distance"),
                    KickOffTime = reader.GetVal<DateTime>("KickOffTime")
                };

                string matchName = eventType == Outright
                    ? _cacheRefAdapter.GetTeamName(matchInfo.HomeId)
                    : GetMatchName(matchInfo, sportType);

                var item = new LeagueWinLoss
                {
                    MatchId = matchId,
                    MatchName = matchName,
                    EventType = eventType,
                    HomeId = matchInfo.HomeId,
                    AwayId = matchInfo.AwayId,
                };
                ReadCommonData(reader, item);

                data.Add(item);
            }

            BuildLeagueTotalData(data, dataList, leagueId, sportType, isHideSaba);

            return new SortableReportResultModel<LeagueWinLoss>
            {
                DataList = dataList,
                TotalData = GetSumData(data)
            };
        }

        private string GetMatchName(MatchInfo matchInfo, int sportType)
        {
            if (SportHelper.IsVSSportTypes(sportType))
            {
                return string.Format(EnglishCulture, KickOfTimeFormat, matchInfo.KickOffTime);
            }

            if (SportHelper.IsVSNewRacingSportTypes(sportType))
            {
                var id = !string.IsNullOrEmpty(matchInfo.Distance) ? $" - ID {matchInfo.Distance}" : string.Empty;
                return $"{ViewResources.Event} {matchInfo.VendorMatchId}{id}";
            }

            return _cacheRefAdapter.GetMatchName(matchInfo.HomeId, matchInfo.AwayId, matchInfo.IsNeutral, sportType);
        }

        private void BuildLeagueTotalData(List<LeagueWinLoss> data, SortableList<LeagueWinLoss> dataList, int leagueId, int sportType, bool isHideSaba)
        {
            var leagueData = GetSumData(data);
            leagueData.Detail = data.OrderBy(_ => _.EventType).ThenBy(_ => _.MatchId);
            leagueData.LeagueName = GetLeagueName(leagueId, isHideSaba, sportType);
            dataList.Add(leagueData);
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetBettypeWinlossDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_BetType";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue(MatchIdColumn, metaModel.MatchId);
            db.SetParameterValue(EventTypeColumn, metaModel.EventType);
            db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
            db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
            UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay, true);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return GetBetTypeData(reader, metaModel);
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
                    BetTypeName = _cacheRefAdapter.GetBettype(betTypeId, metaModel.IsHideSaba),
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

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue("leagueId", metaModel.LeagueId);
            db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
            UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return GetCurrencyData(reader);
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetMatchWinlossCurrencyDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_Match_Currency";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue(MatchIdColumn, metaModel.MatchId);
            db.SetParameterValue(EventTypeColumn, metaModel.EventType);
            db.SetParameterValue(BetTypeListColumn, metaModel.BetTypeIdList);
            db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
            UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return GetCurrencyData(reader);
        }

        public async Task<SortableReportResultModel<LeagueWinLoss>> GetBettypeWinlossCurrencyDetail(LeagueWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_WinLossByLeague_BetType_Currency";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using var db = ObjectDbFactory.CreateInstance(UspName);
            db.SetParameterValue(MatchIdColumn, metaModel.MatchId);
            db.SetParameterValue(EventTypeColumn, metaModel.EventType);
            db.SetParameterValue("betType", metaModel.BettypeId);
            db.SetParameterValue(BalanceUpDayColumn, balanceUpInfo.BalanceUpDay);
            UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return GetCurrencyData(reader);
        }
    }
}