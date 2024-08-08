namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using Data;
    using DTOs;
    using Fanex.Common.Extensions;
    using Fanex.Core.Utils;
    using Interfaces;
    using MetaModels;

    public class ThirdPartyWinLossRepository<T> : IThirdPartyWinLossRepository<T> where T : ThirdPartyWinLoss
    {
        public async Task<ReportResultModel<T>> GetSummaryWinloss(
            ThirdPartyWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList,
            string spname)
        {
            using (var db = ObjectDbFactory.CreateInstance(spname))
            {
                UpdateUspParams(db, metaModel);
                db.SetParameterValue("ReportType", metaModel.ReportBy);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var result = GetData(reader, currencyList, metaModel, -1);
                    AddRowSpan(result.DataList);
                    return result;
                }
            }
        }

        public async Task<ReportResultModel<T>> GetCustomerWinlossDetail(
            ThirdPartyWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList,
            string spname)
        {
            using (var db = ObjectDbFactory.CreateInstance(spname))
            {
                db.SetParameterValue("custid", metaModel.DrillDownCustomerId);
                UpdateUspParams(db, metaModel);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel);
                }
            }
        }

        public async Task<ReportResultModel<T>> GetAgentWinlossDetail(
            ThirdPartyWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList,
            string spname)
        {
            using (var db = ObjectDbFactory.CreateInstance(spname))
            {
                UpdateUspParams(db, metaModel);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel);
                }
            }
        }

        [CLSCompliant(false)]
        protected virtual void UpdateUspParams(IObjectDb db, ThirdPartyWinLossMetaModel metaModel)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("DefaultBaseCurrency", metaModel.DefaultBaseCurrency);

            if (metaModel.ProductId > 0)
            {
                db.SetParameterValue("productid", metaModel.ProductId);
            }

            if (metaModel.MoveBetsDay != null)
            {
                db.SetParameterValue("MoveBetsDay", metaModel.MoveBetsDay);
            }
        }

        protected virtual void ReadSummaryTotal(
            IDataReader reader,
            IDictionary<string, T> summaryTotal,
            string defaultBaseCurrency)
        {
            foreach (var summary in summaryTotal)
            {
                var suffixColumn =
                    !string.IsNullOrEmpty(defaultBaseCurrency) &&
                    summary.Key.Equals(defaultBaseCurrency, StringComparison.OrdinalIgnoreCase)
                        ? "Base"
                        : summary.Key;

                var item = summary.Value;
                item.CustomerWinLoss += reader.ToDecimal("winlost" + suffixColumn);
                item.TotalTransaction += reader.ToInt("totaltrans");
                item.LicenseeWL += reader.ToDecimal("licenseewinlost" + suffixColumn);
                item.TurnOver += reader.ToDecimal("turnover" + suffixColumn);
                item.ContributionWL += reader.ToDecimal("ContributionWinlost" + suffixColumn);
                item.PromotionWL += reader.ToDecimal("PromotionWinlost" + suffixColumn);
                item.JackpotWL += reader.ToDecimal("JackpotWinlost" + suffixColumn);
                item.TotalWL += reader.ToDecimal("TotalLicenseeWinlost" + suffixColumn);
            }
        }

        protected virtual void PostReadItem(IDataReader reader, T item)
        {
            // Read other columns for other products.
        }

        protected virtual IEnumerable<T> CalculateTotal(IEnumerable<T> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new List<T>();
            }

            return dataList.GroupBy(d => d.CurrencyName).Select(g =>
            {
                var item = Activator.CreateInstance<T>();
                item.CurrencyName = g.Key;
                item.TurnOver = g.Sum(p => p.TurnOver);
                item.TotalTransaction = g.Sum(p => p.TotalTransaction);
                item.CustomerWinLoss = g.Sum(p => p.CustomerWinLoss);
                item.LicenseeWL = g.Sum(p => p.LicenseeWL);
                item.ContributionWL = g.Sum(p => p.ContributionWL);
                item.PromotionWL = g.Sum(p => p.PromotionWL);
                item.JackpotWL = g.Sum(p => p.JackpotWL);
                item.TotalWL = g.Sum(p => p.TotalWL);
                return item;
            }).ToList();
        }

        private static void AddRowSpan(IEnumerable<T> dataList)
        {
            DateTime? date = DateTimeHelper.CreateDateTime();
            var index = 1;
            var i = 0;
            var total = dataList.Count();

            while (i < total)
            {
                var item = dataList.ElementAt(i);
                date = item.Date;

                item.RowSpan = dataList.Count(x => x.Date == date);
                item.Index = index;

                index++;
                i += item.RowSpan;
            }
        }

        private ReportResultModel<T> GetData(
            IDataReader reader,
            IReadOnlyDictionary<int, string> currencyList,
            ThirdPartyWinLossMetaModel metaModel,
            int rowSpan = 0)
        {
            var result = new SortableList<T>();

            var summaryTotal =
                SystemConstants.BaseCurrencyListOfSummaryTotalRow.ToDictionary(currency => currency,
                    _ => Activator.CreateInstance<T>());

            if (!summaryTotal.ContainsKey(metaModel.DefaultBaseCurrency))
            {
                summaryTotal.Add(metaModel.DefaultBaseCurrency, Activator.CreateInstance<T>());
            }

            var hasSystemId = reader.HasColumn("username");
            var hasExternalId = reader.HasColumn("nickname");
            var hasCustomerId = reader.HasColumn("custid");
            var hasWinlostDate = reader.HasColumn("winlostdate");

            var index = 1;

            while (reader.Read())
            {
                var currencyId = reader.ToInt("currency");
                var item = Activator.CreateInstance<T>();

                item.Index = index++;
                item.SystemId = hasSystemId ? reader.ToString("username") : string.Empty;
                item.ExternalId = hasExternalId ? reader.ToString("nickname") : string.Empty;
                item.CustomerId = hasCustomerId ? reader.ToInt("custid") : 0;
                item.CurrencyId = currencyId;
                item.CurrencyName = currencyList?.GetValueOrDefault(currencyId, currencyId.ToString());
                item.CustomerWinLoss = reader.ToDecimal("winlost");
                item.LicenseeWL = reader.ToDecimal("licenseewinlost");
                item.TurnOver = reader.ToDecimal("turnover");
                item.ContributionWL = reader.ToDecimal("ContributionWinlost");
                item.PromotionWL = reader.ToDecimal("PromotionWinlost");
                item.JackpotWL = reader.ToDecimal("JackpotWinlost");
                item.TotalWL = reader.ToDecimal("TotalLicenseeWinlost");
                item.TotalTransaction = reader.ToInt("totaltrans");
                item.Date = hasWinlostDate ? reader.ToDateTime("winlostdate") as DateTime? : metaModel.FromDate;
                item.RowSpan = rowSpan;
                PostReadItem(reader, item);

                result.Add(item);
                ReadSummaryTotal(reader, summaryTotal, metaModel.DefaultBaseCurrency);
            }

            return new ReportResultModel<T>
            {
                DataList = result,
                TotalDataList = CalculateTotal(result),
                SummaryTotal = summaryTotal
            };
        }
    }
}