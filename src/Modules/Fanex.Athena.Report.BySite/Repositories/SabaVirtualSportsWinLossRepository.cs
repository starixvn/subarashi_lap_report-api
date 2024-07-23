namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.DTO;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Data;
    using DTOs;
    using Fanex.Common.Extensions;
    using Fanex.Core.Utils;
    using Interfaces;
    using MetaModels;
    using Resources.Constants;

    public class SabaVirtualSportsWinLossRepository : ISabaVirtualSportsWinLossRepository
    {
        private readonly IBalanceUpService _balanceUpDataService;
        private readonly string[] _ignoredLicenseeWlProducts = { Product.BA };
        private readonly string[] _ignoredTurnoverProducts = { Product.BT, Product.CO };

        public SabaVirtualSportsWinLossRepository(IBalanceUpService balanceUpDataService)
        {
            _balanceUpDataService = balanceUpDataService;
        }

        public async Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetSummaryWinlossAsync(
            SabaVirtualSportsWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            const string UspName = "Lrf_Rpt_SabaVirtualSportsWinLoss";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("reporttype", metaModel.ReportBy);
                await UpdateUspParams(db, metaModel);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel, false);
                }
            }
        }

        public async Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetCustomerWinlossDetailAsync(
            SabaVirtualSportsWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            const string UspName = "Lrf_Rpt_SabaVirtualSportsWinLoss_Mem";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("AgentId", metaModel.CustId);
                await UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel, true);
                }
            }
        }

        public async Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetAgentWinlossDetailAsync(
            SabaVirtualSportsWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            const string UspName = "Lrf_Rpt_SabaVirtualSportsWinLoss_Ag";
            const string CustomerColumn = "Recommend";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                await UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel, true, CustomerColumn);
                }
            }
        }

        private static void ReadData(IDataReader reader, SabaVirtualSportsWinLossDetail data, string suffixColumn = "")
        {
            data.CustomerWinLoss = reader.ToDecimal("winlost" + suffixColumn);
            data.CustomerComm = reader.ToDecimal("playercomm" + suffixColumn);
            data.TotalTransaction = reader.ToInt("betcount");
            data.CashOutCommission = reader.ToDecimal("cashoutcomm" + suffixColumn);
            data.BACommission = reader.ToDecimal("bacomm" + suffixColumn);
            data.AmtDueToBA = reader.ToDecimal("amtduetoba" + suffixColumn);
        }

        private static IDictionary<string, decimal> ReadProductDataList(
            IDataReader reader,
            IDictionary<string, string> columns)
        {
            var data = new Dictionary<string, decimal>();

            foreach (var column in columns)
            {
                data.Add(column.Key, reader.ToDecimal(column.Value));
            }

            return data;
        }

        private bool IsIgnoredLicenseeWlProduct(string productCode)
        {
            return _ignoredLicenseeWlProducts.Contains(productCode);
        }

        private bool IsIgnoredTurnoverProduct(string productCode)
        {
            return _ignoredTurnoverProducts.Contains(productCode);
        }

        private IDictionary<string, SabaVirtualSportsWinLossDetail> GetSummaryTotal(
            IDataReader reader,
            SabaVirtualSportsWinLossMetaModel metaModel,
            bool hasData)
        {
            var summaryTotal = new Dictionary<string, SabaVirtualSportsWinLossDetail>();
            var baseCurrencies = SystemConstants.BaseCurrencyListOfSummaryTotalRow.Select(x => new { Name = x, SuffixColumn = x }).ToList();
            if (!string.IsNullOrEmpty(metaModel.DefaultBaseCurrency) &&
                !SystemConstants.BaseCurrencyListOfSummaryTotalRow.Any(x =>
                    x.Equals(metaModel.DefaultBaseCurrency, StringComparison.OrdinalIgnoreCase)))
            {
                baseCurrencies.Add(new { Name = metaModel.DefaultBaseCurrency.ToUpperInvariant(), SuffixColumn = string.Empty });
            }

            foreach (var currency in baseCurrencies)
            {
                var data = hasData
                    ? ReadSummaryTotal(reader, metaModel.ProductOptions, currency.SuffixColumn)
                    : new SabaVirtualSportsWinLossDetail
                    {
                        LicenseeWL = new Dictionary<string, decimal>(),
                        TurnOver = new Dictionary<string, decimal>()
                    };

                summaryTotal.Add(currency.Name, data);
            }

            return summaryTotal;
        }

        private SabaVirtualSportsWinLossDetail ReadSummaryTotal(IDataReader reader, IList<ProductOption> productOptions, string suffixColumn = "")
        {
            var licenseeWLColumns = GetLicenseeWLColumn(reader, productOptions, suffixColumn);
            var turnoverColumns = GetTurnoverColumn(reader, productOptions, suffixColumn);
            var data = new SabaVirtualSportsWinLossDetail
            {
                LicenseeWL = ReadProductDataList(reader, licenseeWLColumns),
                TurnOver = ReadProductDataList(reader, turnoverColumns)
            };
            ReadData(reader, data, suffixColumn);

            return data;
        }

        private IEnumerable<SabaVirtualSportsWinLossDetail> CalculateTotal(IEnumerable<SabaVirtualSportsWinLossDetail> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new List<SabaVirtualSportsWinLossDetail>();
            }

            return dataList.GroupBy(d => d.CurrencyName).Select(g => new SabaVirtualSportsWinLossDetail
            {
                CurrencyName = g.Key,
                TurnOver = g.SelectMany(k => k.TurnOver).GroupBy(j => j.Key).ToDictionary(p => p.Key, p => p.Sum(j => j.Value)),
                TotalTransaction = g.Sum(p => p.TotalTransaction),
                CustomerWinLoss = g.Sum(p => p.CustomerWinLoss),
                CustomerComm = g.Sum(p => p.CustomerComm),
                LicenseeWL = g.SelectMany(k => k.LicenseeWL).GroupBy(j => j.Key).ToDictionary(p => p.Key, p => p.Sum(j => j.Value)),
                CashOutCommission = g.Sum(p => p.CashOutCommission),
                BACommission = g.Sum(p => p.BACommission),
                AmtDueToBA = g.Sum(p => p.AmtDueToBA)
            }).ToList();
        }

        private async Task UpdateUspParams(IObjectDb db, SabaVirtualSportsWinLossMetaModel metaModel)
        {
            db.SetParameterValue("SportType", metaModel.Sport);
            db.SetParameterValue("LeagueId", metaModel.League);
            db.SetParameterValue("UserName", metaModel.UserName ?? string.Empty);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("drawfilter", metaModel.DrawFilter);
            db.SetParameterValue("movebetsday", (await _balanceUpDataService.GetBalanceUpAsync()).MoveBetsDay);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("DefaultBaseCurrency", metaModel.DefaultBaseCurrency);
        }

        private SortableReportResultModel<SabaVirtualSportsWinLossDetail> GetData(
            IDataReader reader,
            IReadOnlyDictionary<int, string> currencyList,
            SabaVirtualSportsWinLossMetaModel metaModel,
            bool drilldown,
            string customerColumn = "CustId")
        {
            var dataList = new SortableList<SabaVirtualSportsWinLossDetail>();
            var licenseeWLColumns = GetLicenseeWLColumn(reader, metaModel.ProductOptions);
            var turnoverColumns = GetTurnoverColumn(reader, metaModel.ProductOptions);

            var hasSystemId = reader.HasColumn("username");
            var hasExternalId = reader.HasColumn("nickname");
            var hasCustomerId = reader.HasColumn(customerColumn);
            var isMonthly = metaModel.ReportBy == ReportByType.Monthly;

            var index = 0;
            while (reader.Read())
            {
                index++;
                var currencyId = reader.ToInt("currency");
                var item = new SabaVirtualSportsWinLossDetail
                {
                    Index = index,
                    SystemId = hasSystemId ? reader.ToString("username") : string.Empty,
                    ExternalId = hasExternalId ? reader.ToString("nickname") : string.Empty,
                    CustomerId = hasCustomerId ? reader.ToInt(customerColumn) : 0,
                    CurrencyId = currencyId,
                    CurrencyName = currencyList?.GetValueOrDefault(currencyId, currencyId.ToString()),
                    Date = metaModel.DrillDownLevel > 0 ? metaModel.FromDate : ConvertHelper.ToDateTime(reader.ToString("reporttype"), isMonthly),
                    LicenseeWL = ReadProductDataList(reader, licenseeWLColumns),
                    TurnOver = ReadProductDataList(reader, turnoverColumns)
                };
                ReadData(reader, item);

                dataList.Add(item);
            }

            var hasSummaryData = reader.NextResult() && reader.Read();

            return new SortableReportResultModel<SabaVirtualSportsWinLossDetail>
            {
                DataList = dataList,
                TotalDataList = CalculateTotal(dataList),
                SummaryTotal = GetSummaryTotal(reader, metaModel, hasSummaryData)
            };
        }

        private IDictionary<string, string> GetLicenseeWLColumn(IDataReader reader, IList<ProductOption> products, string suffix = "")
        {
            var licenseeWLColumns = new Dictionary<string, string>();

            foreach (var product in products)
            {
                var licenseeWlColumnName = "LicenseeWL" + product.Code + suffix;

                if (!IsIgnoredLicenseeWlProduct(product.Code) && reader.HasColumn(licenseeWlColumnName))
                {
                    licenseeWLColumns.Add(product.Code, licenseeWlColumnName);
                }
            }

            return licenseeWLColumns;
        }

        private IDictionary<string, string> GetTurnoverColumn(IDataReader reader, IList<ProductOption> products, string suffix = "")
        {
            var turnoverColumns = new Dictionary<string, string>();

            foreach (var product in products)
            {
                var turnOverColumnName = "turnover" + product.Code + suffix;

                if (!IsIgnoredTurnoverProduct(product.Code) && reader.HasColumn(turnOverColumnName))
                {
                    turnoverColumns.Add(product.Code, turnOverColumnName);
                }
            }

            return turnoverColumns;
        }
    }
}