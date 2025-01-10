using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Repository.Interfaces;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Common.Extensions;
using Fanex.Data;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Repository
{
    public class CustomerTurnoverRepository : ICustomerTurnoverRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public CustomerTurnoverRepository(IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<ReportResultModel<CustTurnover>> GetDataList(CustomerTurnoverMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string languageCode)
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
                        var turnover = reader.ToDecimal("Turnover");
                        var sportId = reader.ToInt("sporttype");
                        var currencyid = reader.ToInt("currencyid");
                        var bettype = reader.ToString("Bettype");
                        var bettypeMatch = Regex.Match(bettype, @"(^[^-\n]*) - ([^-\n]*$)");
                        if (bettypeMatch.Success)
                        {
                            bettype = $"{ProductHelper.GetProductName(bettypeMatch.Groups[ProductPrefixPosition].Value)} - {bettypeMatch.Groups[BettypePosition].Value}";
                        }

                        result.Add(new CustTurnover
                        {
                            Index = reader.ToInt("RowNo"),
                            ReportBy = ConvertHelper.ToDateTime(reader.ToString("ReportBy"), metaModel.ReportBy == ReportByType.Monthly),
                            ExternalId = reader.ToString("nickname"),
                            SystemId = reader.ToString("username"),
                            Currency = currencyList?.GetValueOrDefault(currencyid, currencyid.ToString()),
                            SportId = sportId,
                            SportType = GetSportName(sportId, languageCode),
                            TotalTransactions = reader.ToInt("BetCount"),
                            Bettype = bettype,
                            Turnover = turnover,
                            TotalTurnover = turnover,
                            CustomerWinLoss = reader.ToDecimal("Winlost"),
                            EffectiveBettingValue = reader.ToDecimal("BetEffective"),
                            CustomerComm = reader.ToDecimal("PlayerComm"),
                        });
                    }

                    CustTurnover totalData = null;
                    int totalRows = 0;
                    if (reader.NextResult() && reader.Read())
                    {
                        totalData = ReadTotalData(reader);
                        totalRows = reader.ToInt("TotalRows");
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
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(languageCode);
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
                TotalTransactions = reader.ToInt("SumBetCount"),
                Turnover = reader.ToDecimal("SumTurnover"),
                TotalTurnover = reader.ToDecimal("SumTurnover"),
                CustomerWinLoss = reader.ToDecimal("SumWinlost"),
                CustomerComm = reader.ToDecimal("SumComm"),
                EffectiveBettingValue = reader.ToDecimal("SumBetEffective")
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
