namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.DataDomain;
    using Common.Services.Interfaces;
    using Constants;
    using Core.Models.DTOs;
    using Core.Utility;
    using Data;
    using DTOs;
    using Fanex.Common.Extensions;
    using Interfaces;
    using MetaModels;
    using Resources.Constants;

    public class LiveReportRepository : ILiveReportRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public LiveReportRepository(ICacheRefAdapter cacheRefAdapter, IBalanceUpService balanceUpService)
        {
            _cacheRefAdapter = cacheRefAdapter;
            _balanceUpService = balanceUpService;
        }

        public async Task<IList<LiveReport>> GetLiveReportSummary(LiveReportMetaModel metaModel)
        {
            var uspName = IsSubsidiarySearch(metaModel) ? "Lrf_Rpt_LiveReport" : "Lrf_Rpt_LiveReportSummary";
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            var moveBetsDate = balanceUp.MoveBetsDay;

            using (var db = ObjectDbFactory.CreateInstance(uspName))
            {
                AddParameters(db, metaModel, moveBetsDate);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var list = new List<LiveReport>();

                    while (reader.Read())
                    {
                        var productCode = reader.ToString("ProductCode");

                        var item = new LiveReport
                        {
                            ProductName = ProductHelper.GetProductName(productCode),
                            ProductCode = productCode
                        };
                        ReadCommonColumns(reader, item);
                        list.Add(item);
                    }

                    return list;
                }
            }
        }

        public async Task<IList<LiveReportBySport>> GetDetailBySport(LiveReportMetaModel metaModel)
        {
            var uspName = IsSubsidiarySearch(metaModel) ? "Lrf_Rpt_LiveReportBySportType" : "Lrf_Rpt_LiveReportBySport";
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            var moveBetsDate = balanceUp.MoveBetsDay;

            var list = new List<LiveReportBySport>();

            using (var db = ObjectDbFactory.CreateInstance(uspName))
            {
                AddParameters(db, metaModel, moveBetsDate);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        var sportType = reader.ToString("SportType");
                        var item = new LiveReportBySport
                        {
                            ProductName = GetProductNameBySport(sportType),
                            ProductCode = sportType
                        };

                        var sportId = string.IsNullOrEmpty(item.ProductCode) ? 0 : Convert.ToInt32(item.ProductCode);
                        item.IsLive = !ReportConstant.LiveReportNoDetailLinkSports.Contains(sportId);

                        ReadCommonColumns(reader, item);
                        list.Add(item);
                    }

                    return list;
                }
            }
        }

        public async Task<Tuple<IList<LiveReport>, IList<LiveReportByBetType>>> GetDetailByCurrencyAndBettype(
            LiveReportMetaModel metaModel,
            IDictionary<int, string> currencyList,
            IEnumerable<ProductInfo> products)
        {
            var uspName = IsSubsidiarySearch(metaModel) ? "Lrf_Rpt_LiveReportDetail" : "Lrf_Rpt_LiveReportSummaryDetail";
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            var moveBetsDate = balanceUp.MoveBetsDay;

            using (var db = ObjectDbFactory.CreateInstance(uspName))
            {
                AddParameters(db, metaModel, moveBetsDate);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var resultByCurrency = ReadDetailByCurrency(reader, currencyList);

                    var resultByBettype = reader.NextResult() ? ReadDetailByBettype(reader, products) : new List<LiveReportByBetType>();

                    return new Tuple<IList<LiveReport>, IList<LiveReportByBetType>>(resultByCurrency, resultByBettype);
                }
            }
        }

        public async Task<SortableList<LiveReportWinLossByMatch>> GetLiveReportWinlossByMatch(
            LiveReportMatchListMetalModel metaModel)
        {
            const string UspName = "Lrf_Rpt_LiveReportWinlossByMatch";
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            var moveBetsDate = balanceUp.MoveBetsDay;

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("AgentList", metaModel.AgentListXml);
                db.SetParameterValue("fromdate", metaModel.FromDate);
                db.SetParameterValue("todate", metaModel.ToDate);
                db.SetParameterValue("movebetsday", moveBetsDate);
                db.SetParameterValue("sportid", metaModel.SportId);
                db.SetParameterValue("liveindicator", metaModel.LiveIndicator);
                db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
                db.SetParameterValue("ProductList", metaModel.ProductCodeList);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var data = new SortableList<LiveReportWinLossByMatch>();

                    while (reader.Read())
                    {
                        data.Add(new LiveReportWinLossByMatch
                        {
                            MatchId = reader.GetVal<long>("matchid"),
                            HomeId = reader.GetVal<int>("homeid"),
                            AwayId = reader.GetVal<int>("awayid"),
                            BetCount = reader.GetVal<int>("Betcount"),
                            TurnOver = reader.GetVal<decimal>("Turnover"),
                            CustomerWinLoss = reader.GetVal<decimal>("Winlost"),
                            CustomerComm = reader.GetVal<decimal>("PlayerComm"),
                            LicenseeComm = reader.GetVal<decimal>("Comm"),
                            LicenseeWinLoss = reader.GetVal<decimal>("LicenseeWL"),
                            EventType = reader.GetVal<int>("EventType"),
                            VendorMatchId = reader.ToString("VendorMatchId"),
                            Distance = reader.ToString("Distance"),
                            SportId = metaModel.SportId
                        });
                    }

                    return data;
                }
            }
        }

        protected internal virtual string GetProductNameBySport(string sportType)
        {
            // 101 -> 104: const from SP to separate names for special bettypes
            const int MixParlaySportType = 101;
            const int CombinationMixParlaySportType = 102;
            const int OutrightSportType = 103;
            const int VirtualSports2MixParlaySportType = 104;

            var sportId = string.IsNullOrEmpty(sportType) ? 0 : Convert.ToInt32(sportType);

            switch (sportId)
            {
                case MixParlaySportType:
                case VirtualSports2MixParlaySportType:
                    return _cacheRefAdapter.GetBettype(BetType.MixParlay);

                case CombinationMixParlaySportType:
                    return _cacheRefAdapter.GetBettype(BetType.CombinationMixParlay);

                case OutrightSportType:
                    return _cacheRefAdapter.GetBettype(BetType.Outright);

                default:
                    return _cacheRefAdapter.GetSportName(sportType);
            }
        }

        private static void AddParameters(IObjectDb db, LiveReportMetaModel metaModel, DateTime moveBetsDate)
        {
            db.SetParameterValue("ProductCodes", metaModel.ProductCodeList);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate);
            db.SetParameterValue("todate", metaModel.ToDate);
            db.SetParameterValue("movebetsday", moveBetsDate);
            db.SetParameterValue("liveindicator", metaModel.LiveIndicator);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
        }

        private static void ReadCommonColumns(IDataReader reader, LiveReport item)
        {
            item.TurnOver = reader.GetVal<decimal>("Turnover");
            item.CustomerWinLoss = reader.GetVal<decimal>("Winlost");
            item.CustomerComm = reader.GetVal<decimal>("PlayerComm");
            item.LicenseeComm = reader.GetVal<decimal>("Comm");
            item.LicenseeWinLoss = reader.GetVal<decimal>("LicenseeWL");
            item.AmountDue = reader.GetVal<decimal>("AmtDueto3rd");
            item.BetCount = reader.GetVal<int>("Betcount");
        }

        private IList<LiveReport> ReadDetailByCurrency(IDataReader reader, IDictionary<int, string> currencyList)
        {
            var list = new List<LiveReport>();

            while (reader.Read())
            {
                var currencyId = reader.GetVal<int>("Currency");
                var currencyName = currencyList?.GetValueOrDefault(currencyId, currencyId.ToString());
                var item = new LiveReport
                {
                    ProductName = currencyName,
                    ProductCode = currencyName
                };

                ReadCommonColumns(reader, item);
                list.Add(item);
            }

            return list.OrderBy(x => x.ProductName).ToList();
        }

        private IList<LiveReportByBetType> ReadDetailByBettype(IDataReader reader, IEnumerable<ProductInfo> products)
        {
            var dataList = new List<LiveReport>();

            while (reader.Read())
            {
                var item = new LiveReport
                {
                    ProductName = GetBetTypeName(reader.GetVal<int>("bettype")),
                    ProductCode = reader.GetVal<string>("ProductCode")
                };
                ReadCommonColumns(reader, item);

                dataList.Add(item);
            }

            var result = new List<LiveReportByBetType>();

            if (dataList.Any())
            {
                result = products.Join(
                        dataList,
                        p => p.ProductCode,
                        d => d.ProductCode,
                        (p, d) => new { p.ProductName, LiveReports = d })
                    .GroupBy(
                        x => x.ProductName,
                        (key, g) => new LiveReportByBetType
                        {
                            ProductName = key,
                            LiveReports = g.Select(x => x.LiveReports).ToList()
                        }).ToList();
            }

            return result;
        }

        private string GetBetTypeName(int betType)
        {
            var suffix = GetBettypeNameSuffix(betType);
            return _cacheRefAdapter.GetBettype(betType) + suffix;
        }

        private string GetBettypeNameSuffix(int betType)
        {
            if (betType == BetType.NumberGameHappy5OddEven)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.NumberGameHappy5) + ")";
            }

            if (betType >= BetType.VirtualBasketballOverUnder && betType <= BetType.VirtualBasketballFirstHalfHandicap)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.VirtualBasketball) + ")";
            }

            return string.Empty;
        }

        private static bool IsSubsidiarySearch(LiveReportMetaModel metalModel) => metalModel.SubsidiarySearch;
    }
}