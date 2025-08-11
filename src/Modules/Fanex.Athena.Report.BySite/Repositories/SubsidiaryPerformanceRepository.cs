namespace Fanex.Athena.Report.BySite.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Data;
    using DTOs.SubsidiaryPerformance;
    using Fanex.Common;
    using Interfaces;
    using MetaModels;

    public class SubsidiaryPerformanceRepository : ISubsidiaryPerformanceRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        public SubsidiaryPerformanceRepository(IBalanceUpService balanceUpService)
        {
            _balanceUpService = balanceUpService;
        }

        public async Task<IEnumerable<SubsidiaryPerformance>> GetData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList)
        {
            const string SpName = "Lrf_Rpt_SubsidiaryWinloss";
            var dataList = new List<SubsidiaryPerformance>();

            using (var objectDb = ObjectDbFactory.CreateInstance(SpName))
            {
                await AssignParameter(objectDb, metaModel);
                objectDb.SetParameterValue("SeparateSabaVS", metaModel.IsSeparateSabaWinLoss);

                using (var reader = await objectDb.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        var data = MapData(reader, currencyList, metaModel.ConvertAll);

                        dataList.Add(data);
                    }
                }
            }

            return dataList;
        }

        public async Task<ReportResultModel<SubsidiaryPerformanceDetail>> GetDetailData(SubsidiaryPerformanceMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList)
        {
            const string SpName = "Lrf_Rpt_SubsidiaryWinlossDetail";
            var dataList = new List<SubsidiaryPerformanceDetail>();
            var totalList = new Dictionary<string, SubsidiaryPerformanceDetail>();

            using (var objectDb = ObjectDbFactory.CreateInstance(SpName))
            {
                await AssignParameter(objectDb, metaModel);

                using (var reader = await objectDb.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var baseColumnSuffix = metaModel.ConvertAll ? "Base" : string.Empty;

                    while (reader.Read())
                    {
                        var data = MapDetailData(reader, currencyList, baseColumnSuffix);
                        data.SystemId = reader["username"].ToString();
                        data.ExternalId = reader["nickname"].ToString();
                        dataList.Add(data);

                        if (metaModel.ConvertAll)
                        {
                            CalculateDetailTotal(data, totalList);
                        }
                        else
                        {
                            CalculateDetailTotal(data.CurrencyName, reader, totalList);
                        }
                    }
                }
            }

            return new ReportResultModel<SubsidiaryPerformanceDetail>
            {
                DataList = dataList,
                SummaryTotal = totalList,
                TotalData = CalculateDetailGrandTotal(metaModel.BaseCurrency.ToUpperInvariant(), totalList.Values)
            };
        }

        private SubsidiaryPerformance MapData(IDataReader reader, IReadOnlyDictionary<int, string> currencyList, bool isConvert)
        {
            var currencyId = Converter.To<int>(reader["Currency"]);
            var result = new SubsidiaryPerformance
            {
                ProductCode = reader["ProductCode"].ToString(),
                SubsidiaryId = Converter.To<int>(reader["recommend"]),
                CurrencyName = currencyList?.GetValueOrDefault(currencyId, currencyId.ToString()),
                CurrencyId = currencyId,
                BaseCurrencyData = new SubsidiaryPerformanceBase
                {
                    BetCount = Converter.To<int>(reader["BetCount"]),
                    Turnover = Converter.To<decimal>(reader["TurnoverBase"]),
                    CustomerWinLoss = Converter.To<decimal>(reader["WinlostBase"]),
                    SubsidiaryWinLoss = Converter.To<decimal>(reader["LicenseeWLBase"])
                }
            };
            result.BetCount = result.BaseCurrencyData.BetCount;

            if (isConvert)
            {
                result.Turnover = result.BaseCurrencyData.Turnover;
                result.CustomerWinLoss = result.BaseCurrencyData.CustomerWinLoss;
                result.SubsidiaryWinLoss = result.BaseCurrencyData.SubsidiaryWinLoss;
            }
            else
            {
                result.Turnover = Converter.To<decimal>(reader["Turnover"]);
                result.CustomerWinLoss = Converter.To<decimal>(reader["Winlost"]);
                result.SubsidiaryWinLoss = Converter.To<decimal>(reader["LicenseeWL"]);
            }

            return result;
        }

        private SubsidiaryPerformanceDetail MapDetailData(IDataReader reader, IReadOnlyDictionary<int, string> currencyList, string baseColumnSuffix = "Base")
        {
            var currencyId = Converter.To<int>(reader["Currency"]);
            var result = new SubsidiaryPerformanceDetail
            {
                SystemId = reader["username"].ToString(),
                ExternalId = reader["nickname"].ToString(),
                CurrencyId = currencyId,
                CurrencyName = currencyList?.GetValueOrDefault(currencyId, currencyId.ToString()),
                BetCount = Converter.To<int>(reader["BetCount"]),
                Turnover = Converter.To<decimal>(reader["Turnover" + baseColumnSuffix]),
                CustomerWinLoss = Converter.To<decimal>(reader["Winlost" + baseColumnSuffix]),
                SubsidiaryWinLoss = Converter.To<decimal>(reader["LicenseeWL" + baseColumnSuffix])
            };

            return result;
        }

        private void CalculateDetailTotal(string currencyName, IDataReader reader, IDictionary<string, SubsidiaryPerformanceDetail> totalList)
        {
            if (!totalList.ContainsKey(currencyName))
            {
                totalList.Add(currencyName, new SubsidiaryPerformanceDetail { CurrencyName = currencyName });
            }

            var total = totalList[currencyName];

            total.BetCount += Converter.To<int>(reader["BetCount"]);
            total.Turnover += Converter.To<decimal>(reader["TurnoverBase"]);
            total.CustomerWinLoss += Converter.To<decimal>(reader["WinlostBase"]);
            total.SubsidiaryWinLoss += Converter.To<decimal>(reader["LicenseeWLBase"]);
        }

        private void CalculateDetailTotal(SubsidiaryPerformanceDetail detail, IDictionary<string, SubsidiaryPerformanceDetail> totalList)
        {
            if (!totalList.ContainsKey(detail.CurrencyName))
            {
                totalList.Add(detail.CurrencyName, new SubsidiaryPerformanceDetail { CurrencyName = detail.CurrencyName });
            }

            var total = totalList[detail.CurrencyName];

            total.BetCount += detail.BetCount;
            total.Turnover += detail.Turnover;
            total.CustomerWinLoss += detail.CustomerWinLoss;
            total.SubsidiaryWinLoss += detail.SubsidiaryWinLoss;
        }

        private SubsidiaryPerformanceDetail CalculateDetailGrandTotal(string baseCurrency, IEnumerable<SubsidiaryPerformanceDetail> total)
        {
            return new SubsidiaryPerformanceDetail
            {
                CurrencyName = baseCurrency,
                BetCount = total.Sum(x => x.BetCount),
                Turnover = total.Sum(x => x.Turnover),
                CustomerWinLoss = total.Sum(x => x.CustomerWinLoss),
                SubsidiaryWinLoss = total.Sum(x => x.SubsidiaryWinLoss)
            };
        }

        private async Task AssignParameter(IObjectDb objectDb, SubsidiaryPerformanceMetaModel metaModel)
        {
            objectDb.SetParameterValue("FromDate", metaModel.FromDate);
            objectDb.SetParameterValue("ToDate", metaModel.ToDate);
            objectDb.SetParameterValue("MoveBetsDay", (await _balanceUpService.GetBalanceUpAsync()).MoveBetsDay);
            objectDb.SetParameterValue("ProductList", metaModel.ProductCodeList);
            objectDb.SetParameterValue("IncludeRaw", metaModel.IncludeRaw);
            objectDb.SetParameterValue("AgentList", metaModel.AgentListXml);
            objectDb.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
        }
    }
}
