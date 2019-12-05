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
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Data;
    using Fanex.RemotingRef;

    public class PromotionPrizeRepository : IPromotionPrizeRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        private readonly ICurrencyService _currencyService;

        private readonly IRemotingRefService _remotingRefService;

        private readonly IDictionary<string, string> _betTypeNames = new Dictionary<string, string>();

        public PromotionPrizeRepository(
            IBalanceUpService balanceUpService,
            ICurrencyService currencyService,
            IRemotingRefService remotingRefService)
        {
            _balanceUpService = balanceUpService;
            _currencyService = currencyService;
            _remotingRefService = remotingRefService;
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
                    return ReadData(reader, await currencyListTask);
                }
            }
        }

        private IDictionary<string, PromotionPrizeResultModel> ReadData(IDataReader reader, IEnumerable<Currency> currencyList)
        {
            var groupData = ReadSummaryData(reader);

            reader.NextResult();

            while (reader.Read())
            {
                var productCode = reader.GetVal<string>("ProductCode");
                groupData[productCode].Data.Add(ReadMainData(reader, currencyList));
            }

            return groupData;
        }

        private IDictionary<string, PromotionPrizeResultModel> ReadSummaryData(IDataReader reader)
        {
            var data = new Dictionary<string, PromotionPrizeResultModel>();
            while (reader.Read())
            {
                var productCode = reader.GetVal<string>("ProductCode");
                var summary = new PromotionPrize
                {
                    PromotionType = this.GetBetTypeName(reader.GetVal<string>("BetType")),
                    Amount = reader.GetVal<decimal>("SumAmount")
                };

                if (!data.TryAdd(
                    productCode,
                    new PromotionPrizeResultModel
                    {
                        SummaryTotal = new List<PromotionPrize> { summary },
                        Data = new List<PromotionPrize>(),
                        GrandTotal = summary.Amount
                    }))
                {
                    data[productCode].GrandTotal += summary.Amount;
                    data[productCode].SummaryTotal.Add(summary);
                }
            }

            return data;
        }

        private PromotionPrize ReadMainData(IDataReader reader, IEnumerable<Currency> currencyList)
        {
            var currencyId = reader.GetVal<int>("CurrencyId");
            return new PromotionPrize
            {
                SystemId = reader.GetVal<string>("UserName"),
                ExternalId = reader.GetVal<string>("NickName"),
                CurrencyName = currencyList.FirstOrDefault(x => x.ID == currencyId)?.Name,
                Amount = reader.GetVal<decimal>("Amount"),
                Date = reader.GetVal<DateTime>("Date"),
                PromotionType = this.GetBetTypeName(reader.GetVal<string>("BetType"))
            };
        }

        private string GetBetTypeName(string bettype)
        {
            if (_betTypeNames.ContainsKey(bettype))
            {
                return _betTypeNames[bettype];
            }

            var name = _remotingRefService.GetBetTypeName(bettype, Thread.CurrentThread.CurrentUICulture.Name);
            _betTypeNames.Add(bettype, name);

            return name;
        }
    }
}
