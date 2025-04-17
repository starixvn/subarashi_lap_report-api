namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Services.Interfaces;
    using Data;
    using DTOs;
    using Fanex.Common.Extensions;
    using Fanex.Core.Utils.Extensions;
    using Interfaces;
    using MetaModels;

    public class MatchWinLossRepository : IMatchWinLossRepository
    {
        private const string STR_PRODUCTLIST = "ProductList";

        private readonly IBalanceUpService _balanceUpService;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public MatchWinLossRepository(
            IBalanceUpService balanceUpService,
            ICacheRefAdapter cacheRefAdapter)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<IList<MatchWinLoss>> GetDataList(MatchWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_MatchWinLoss";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            await SetCommonParam(db, metaModel);
            db.SetParameterValue("BalanceUpDay", (await _balanceUpService.GetBalanceUpAsync()).BalanceUpDay);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue(STR_PRODUCTLIST, string.Join(",", metaModel.ProductOptions.Where(_ => _.Status).Select(_ => _.Code)));

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return SetToList(reader);
        }

        public async Task<DataSet> GetDetailDataSet(MatchWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_MatchWinLossDetail";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            await SetCommonParam(db, metaModel);
            db.SetParameterValue("BalanceUpDay", (await _balanceUpService.GetBalanceUpAsync()).BalanceUpDay);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("PageSize", metaModel.PageSize);
            db.SetParameterValue(STR_PRODUCTLIST, string.Join(",", metaModel.ProductOptions.Where(_ => _.Status).Select(_ => _.Code)));

            return await db.ExecuteDataSetAsync().ConfigureAwait(false);
        }

        public async Task<IList<MatchWinLoss>> GetDetailDataList(MatchWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_MatchWinLossByBettype";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            await SetCommonParam(db, metaModel);
            db.SetParameterValue("BalanceUpDay", (await _balanceUpService.GetBalanceUpAsync()).BalanceUpDay);
            db.SetParameterValue("MatchId", metaModel.MatchId);
            db.SetParameterValue("EventType", metaModel.EventType);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue(STR_PRODUCTLIST, metaModel.ProductCodeList);

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return SetToList(reader);
        }

        public async Task<IList<MatchWinLoss>> GetOutrightParlayList(MatchWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_MatchWinLossByOutrightParlay";

            using var db = ObjectDbFactory.CreateInstance(UspName);
            await SetCommonParam(db, metaModel);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue(STR_PRODUCTLIST, string.Join(",", metaModel.ProductOptions.Where(_ => _.Status).Select(_ => _.Code)));

            using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
            return SetToListOutrightParlay(reader);
        }

        private IList<MatchWinLoss> SetToListOutrightParlay(IDataReader reader)
        {
            var dataList = new List<MatchWinLoss>();

            while (reader.Read())
            {
                dataList.Add(new MatchWinLoss
                {
                    BetType = reader.ToInt("bettype"),
                    ProductCode = reader.ToString("ProductCode"),
                    Turnover = reader.ToDecimal("Turnover"),
                    TotalTransaction = reader.ToInt("betcount"),
                    CustomerWL = reader.ToDecimal("WinLost"),
                    CustomerComm = reader.ToDecimal("PlayerComm"),
                    LicenseeWL = reader.ToDecimal("LicenseeWL")
                });
            }

            return dataList;
        }

        private IList<MatchWinLoss> SetToList(IDataReader reader)
        {
            var dataList = new List<MatchWinLoss>();

            while (reader.Read())
            {
                var betType = reader.GetVal<int>("bettype");
                var isLive = reader.GetVal<bool>("liveindicator");
                dataList.Add(new MatchWinLoss
                {
                    LeagueId = reader.GetVal<int>("leagueid"),
                    LeagueGroupId = reader.GetVal<int>("LeagueGroupId"),
                    MatchId = reader.GetVal<int>("matchid"),
                    MatchCode = reader.ToString("matchcode"),
                    HomeId = reader.ToInt("homeid"),
                    AwayId = reader.ToInt("awayid"),
                    Turnover = reader.ToDecimal("Turnover"),
                    TotalTransaction = reader.ToInt("betcount"),
                    CustomerWL = reader.ToDecimal("WinLost"),
                    CustomerComm = reader.ToDecimal("PlayerComm"),
                    LicenseeWL = reader.ToDecimal("LicenseeWL"),
                    BetType = betType,
                    BettypeName = _cacheRefAdapter.GetBetTypeWithLiveIndicator(betType, isLive),
                    ProductName = reader.GetVal<string>("productname"),
                    IsLive = isLive,
                    IsNeutral = reader.GetVal<bool>("isNeutral"),
                    SportType = reader.ToShort("sporttype"),
                    KickOffTime = reader.GetVal<DateTime>("KickoffTime"),
                    VendorMatchId = reader.ToString("VendorMatchId"),
                    Distance = reader.ToString("Distance"),
                    EventType = reader.ToInt("EventType")
                });
            }

            return dataList;
        }

        private async Task SetCommonParam(IObjectDb db, MatchWinLossMetaModel metaModel)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("MoveBetsDay", (await _balanceUpService.GetBalanceUpAsync()).MoveBetsDay);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
        }
    }
}