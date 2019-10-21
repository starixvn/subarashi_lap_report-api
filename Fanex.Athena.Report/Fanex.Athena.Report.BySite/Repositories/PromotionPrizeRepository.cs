namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Model.DataDomain;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Data;

    public class PromotionPrizeRepository : IPromotionPrizeRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        private readonly ICurrencyService _currencyService;

        public PromotionPrizeRepository(IBalanceUpService balanceUpService, ICurrencyService currencyService)
        {
            _balanceUpService = balanceUpService;
            _currencyService = currencyService;
        }

        public async Task<IDictionary<string, PromotionPrizeResultModel>> GetData(PromotionPrizeMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_PromotionPrize";

            var balanceUpInfoTask = _balanceUpService.GetBalanceUpAsync();
            var currencyListTask = _currencyService.GetCurrencyListAsync();
            await Task.WhenAll(balanceUpInfoTask, currencyListTask).ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("AgentId", metaModel.AgentId);
                db.SetParameterValue("UserName", metaModel.UserName);
                db.SetParameterValue("FromDate", metaModel.FromDate);
                db.SetParameterValue("ToDate", metaModel.ToDate);
                db.SetParameterValue("MoveBetsDay", (await balanceUpInfoTask).MoveBetsDay);
                db.SetParameterValue("AgentList", metaModel.AgentListXml);
                db.SetParameterValue("DefaultBaseCurrency", metaModel.DefaultBaseCurrency);

                using (var reader = db.ExecuteReader())
                {
                    return ReadData(reader, metaModel.DefaultBaseCurrency, await currencyListTask);
                }
            }
        }

        private static IDictionary<string, PromotionPrize> ReadSummaryData(IDataReader reader, IEnumerable<string> baseCurrencies, string defaultBaseCurrency)
        {
            var data = baseCurrencies.ToDictionary(k => k, v => new PromotionPrize());

            foreach (var total in data)
            {
                var suffixColumn = !string.IsNullOrEmpty(defaultBaseCurrency) && total.Key.Equals(defaultBaseCurrency, StringComparison.OrdinalIgnoreCase)
                                    ? "Base"
                                    : total.Key;

                total.Value.CurrencyName = total.Key;
                total.Value.Amount = Convert.ToDecimal(reader["SumAmount" + suffixColumn]);
            }

            return data;
        }

        private IDictionary<string, PromotionPrizeResultModel> ReadData(IDataReader reader, string defaultBaseCurrency, IEnumerable<Currency> currencyList)
        {
            var summary = new Dictionary<string, PromotionPrizeResultModel>();
            var baseCurrencies = SystemBaseCurrency.DefaultBaseCurrencyList.Contains(defaultBaseCurrency)
                ? SystemBaseCurrency.DefaultBaseCurrencyList
                : SystemBaseCurrency.DefaultBaseCurrencyList.Union(new[] { defaultBaseCurrency });

            while (reader.Read())
            {
                var productCode = reader["ProductCode"].ToString();
                summary.Add(
                    productCode,
                    new PromotionPrizeResultModel
                    {
                        SummaryTotal = ReadSummaryData(reader, baseCurrencies, defaultBaseCurrency),
                        Data = new List<PromotionPrize>()
                    });
            }

            reader.NextResult();

            while (reader.Read())
            {
                var productCode = reader["ProductCode"].ToString();
                summary[productCode].Data.Add(ReadMainData(reader, currencyList));
            }

            return summary;
        }

        private PromotionPrize ReadMainData(IDataReader reader, IEnumerable<Currency> currencyList)
        {
            var currencyId = Convert.ToInt32(reader["CurrencyId"]);
            return new PromotionPrize
            {
                SystemId = reader["UserName"].ToString(),
                ExternalId = reader["NickName"].ToString(),
                CurrencyName = currencyList.FirstOrDefault(x => x.ID == currencyId)?.Name,
                Amount = Convert.ToDecimal(reader["Amount"]),
                Date = Convert.ToDateTime(reader["Date"])
            };
        }
    }
}
