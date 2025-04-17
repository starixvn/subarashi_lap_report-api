namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Core.Services.Interfaces;
    using Data;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Resources.Constants;

    public class GodOfFortuneWinLossRepository : IGodOfFortuneWinLossRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly IContextService _contextService;

        public GodOfFortuneWinLossRepository(
            IBalanceUpService balanceUpService,
            IContextService contextService)
        {
            _balanceUpService = balanceUpService;
            _contextService = contextService;
        }

        public async Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetSummaryWinloss(
            GodOfFortuneWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_GodOfFortuneWinloss";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("reporttype", metaModel.ReportBy);

                UpdateUspParamsAsync(db, metaModel, false, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, metaModel, false);
                }
            }
        }

        public async Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetCustomerWinlossDetail(
            GodOfFortuneWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_GodOfFortuneWinloss_Detail";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("CustId", metaModel.DrillDownCustomerId);

                UpdateUspParamsAsync(db, metaModel, true, balanceUpInfo.MoveBetsDay);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, metaModel, true);
                }
            }
        }

        public async Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetAgentWinlossDetail(
            GodOfFortuneWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_GodOfFortuneWinloss_Ag";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParamsAsync(db, metaModel, true, balanceUpInfo.MoveBetsDay);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, metaModel, true);
                }
            }
        }

        private static void ReadCommonData(IDataReader reader, GodOfFortuneWinLoss data)
        {
            data.CustomerWinLoss = reader.GetVal<decimal>("winlost");
            data.TotalTransaction = reader.GetVal<int>("betcount");
            data.LicenseeWinLoss = reader.GetVal<decimal>("LicenseeWL");
            data.TurnOver = reader.GetVal<decimal>("turnover");
            data.Discount = reader.GetVal<decimal>("discount");
        }

        private static IDictionary<string, GodOfFortuneWinLoss> GetSummaryTotal(IDataReader reader, GodOfFortuneWinLossMetaModel metaModel)
        {
            var hasData = reader.NextResult() && reader.Read();

            var summaryTotal = new Dictionary<string, GodOfFortuneWinLoss>();
            var baseCurrency = !string.IsNullOrEmpty(metaModel.BaseCurrency) ? metaModel.BaseCurrency : metaModel.CurrencyName;

            var data = hasData
                ? ReadSummaryTotal(reader)
                : new GodOfFortuneWinLoss();

            summaryTotal.Add(baseCurrency.ToUpperInvariant(), data);

            return summaryTotal;
        }

        private static GodOfFortuneWinLoss ReadSummaryTotal(IDataReader reader)
        {
            var data = new GodOfFortuneWinLoss();
            ReadCommonData(reader, data);

            return data;
        }

        private IEnumerable<GodOfFortuneWinLoss> GetTotalData(IDataReader reader)
        {
            var total = new List<GodOfFortuneWinLoss>();
            reader.NextResult();

            while (reader.Read())
            {
                var currencyId = reader.GetVal<int>("currency");
                var currencyName = _contextService.CurrencyList.GetValueOrDefault(currencyId, currencyId.ToString());

                var item = new GodOfFortuneWinLoss
                {
                    CurrencyName = currencyName
                };
                ReadCommonData(reader, item);

                total.Add(item);
            }

            return total;
        }

        private static void UpdateUspParamsAsync(IObjectDb db, GodOfFortuneWinLossMetaModel metaModel, bool drilldown, DateTime moveBetsDay)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", drilldown ? metaModel.DrillDownFromDate : metaModel.FromDate.Date);
            db.SetParameterValue("todate", drilldown ? metaModel.DrillDownToDate : metaModel.ToDate.Date);
            db.SetParameterValue("drawfilter", metaModel.DrawFilter);
            db.SetParameterValue("movebetsday", moveBetsDay);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency.ToUpperInvariant());
            db.SetParameterValue("ConvertToBaseCurrency", metaModel.IsConvertAll);
        }

        private SortableReportResultModel<GodOfFortuneWinLoss> GetData(
            IDataReader reader,
            GodOfFortuneWinLossMetaModel metaModel,
            bool drilldown)
        {
            var dataList = new SortableList<GodOfFortuneWinLoss>();
            var isMonthly = metaModel.ReportBy == ReportByType.Monthly;

            var index = 0;
            while (reader.Read())
            {
                index++;
                var currencyId = reader.GetVal<int>("currency");
                var item = new GodOfFortuneWinLoss
                {
                    Index = index,
                    SystemId = reader.GetVal<string>("username"),
                    ExternalId = reader.GetVal<string>("nickname"),
                    CustomerId = reader.GetVal<int>("CustId"),
                    CurrencyId = currencyId,
                    CurrencyName = _contextService.CurrencyList.GetValueOrDefault(currencyId, currencyId.ToString()),
                    Date = drilldown ? metaModel.DrillDownFromDate : ConvertHelper.ToDateTime(reader.GetVal<string>("reporttype"), isMonthly)
                };
                ReadCommonData(reader, item);

                dataList.Add(item);
            }

            return new SortableReportResultModel<GodOfFortuneWinLoss>
            {
                DataList = dataList,
                TotalDataList = GetTotalData(reader),
                SummaryTotal = GetSummaryTotal(reader, metaModel)
            };
        }
    }
}