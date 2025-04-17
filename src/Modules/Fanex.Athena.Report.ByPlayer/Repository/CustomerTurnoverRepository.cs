namespace Fanex.Athena.Report.ByPlayer.Repository
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Constants;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Core.Helpers;
    using Data;
    using DTO;
    using Interfaces;
    using MetaModels;
    using Resources;
    using Resources.Constants;

    public class CustomerTurnoverRepository : ICustomerTurnoverRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public CustomerTurnoverRepository(IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<ReportResultModel<CustTurnover>> GetDataList(
            CustomerTurnoverMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList,
            string languageCode)
        {
            const int ProductPrefixPosition = 1;
            const int BettypePosition = 2;

            using (var db = ObjectDbFactory.CreateInstance("Lrf_Rpt_CustomerTurnOver"))
            {
                await AddParametersAsync(metaModel, languageCode, db);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var result = new List<CustTurnover>();
                    while (reader.Read())
                    {
                        var turnover = reader.GetVal<decimal>("Turnover");
                        var sportId = reader.GetVal<int>("sporttype");
                        var currencyid = reader.GetVal<int>("currencyid");
                        var bettype = reader.GetVal<string>("Bettype");
                        var bettypeMatch = Regex.Match(bettype, @"(^[^-\n]*) - ([^-\n]*$)");
                        if (bettypeMatch.Success)
                        {
                            bettype =
                                $"{ProductHelper.GetProductName(bettypeMatch.Groups[ProductPrefixPosition].Value)} - {bettypeMatch.Groups[BettypePosition].Value}";
                        }

                        result.Add(new CustTurnover
                        {
                            Index = reader.GetVal<int>("RowNo"),
                            ReportBy = ConvertHelper.ToDateTime(reader.GetVal<string>("ReportBy"), metaModel.ReportBy == ReportByType.Monthly),
                            ExternalId = reader.GetVal<string>("nickname"),
                            SystemId = reader.GetVal<string>("username"),
                            Currency = currencyList?.GetValueOrDefault(currencyid, currencyid.ToString()),
                            SportId = sportId,
                            SportType = GetSportName(sportId, languageCode),
                            TotalTransactions = reader.GetVal<int>("BetCount"),
                            Bettype = bettype,
                            Turnover = turnover,
                            TotalTurnover = turnover,
                            CustomerWinLoss = reader.GetVal<decimal>("Winlost"),
                            EffectiveBettingValue = reader.GetVal<decimal>("BetEffective"),
                            CustomerComm = reader.GetVal<decimal>("PlayerComm")
                        });
                    }

                    CustTurnover totalData = null;
                    int totalRows = 0;
                    if (reader.NextResult() && reader.Read())
                    {
                        totalData = ReadTotalData(reader);
                        totalRows = reader.GetVal<int>("TotalRows");
                    }

                    return new ReportResultModel<CustTurnover>
                    {
                        DataList = result,
                        TotalData = totalData ?? new CustTurnover(),
                        TotalRows = totalRows
                    };
                }
            }
        }

        private string GetSportName(int sportId, string languageCode)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            if (sportId == ProductTypes.Parlay)
            {
                return ViewResources.Parlay;
            }

            return _cacheRefAdapter.GetSportName(sportId, languageCode);
        }

        private static CustTurnover ReadTotalData(IDataReader reader)
        {
            return new CustTurnover
            {
                TotalTransactions = reader.GetVal<int>("SumBetCount"),
                Turnover = reader.GetVal<decimal>("SumTurnover"),
                TotalTurnover = reader.GetVal<decimal>("SumTurnover"),
                CustomerWinLoss = reader.GetVal<decimal>("SumWinlost"),
                CustomerComm = reader.GetVal<decimal>("SumComm"),
                EffectiveBettingValue = reader.GetVal<decimal>("SumBetEffective")
            };
        }

        private async Task AddParametersAsync(CustomerTurnoverMetaModel metaModel, string languageCode, IObjectDb db)
        {
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate);
            db.SetParameterValue("todate", metaModel.ToDate);
            db.SetParameterValue("odds", metaModel.Odds);
            db.SetParameterValue("reportby", metaModel.ReportBy);
            db.SetParameterValue("username", string.IsNullOrWhiteSpace(metaModel.Username) ? null : SqlHelper.ToSqlTextSearch(metaModel.Username));
            db.SetParameterValue("movebetsday", balanceUp.MoveBetsDay);
            db.SetParameterValue("Langkey", languageCode);
            db.SetParameterValue("PageSize", metaModel.PageSize);
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("currencyid", metaModel.CurrencyId);
            db.SetParameterValue("IsExportExcel", metaModel.IsExportExcel);
            db.SetParameterValue("showwin", metaModel.ShowWin);
            db.SetParameterValue("showloss", metaModel.ShowLoss);
            db.SetParameterValue("showdraw", metaModel.ShowDraw);
            db.SetParameterValue("Product", metaModel.ProductIds);
            db.SetParameterValue("Bettype", metaModel.Bettypes);
        }
    }
}