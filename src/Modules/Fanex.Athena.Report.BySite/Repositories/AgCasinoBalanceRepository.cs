using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Core.Utils;
using Fanex.Data;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using Fanex.Athena.Report.Core.Utility;
using Fanex.Common.Extensions;

namespace Fanex.Athena.Report.BySite.Repositories
{
    public class AgCasinoBalanceRepository : IAgCasinoBalanceRepository
    {
        public async Task<SortableReportResultModel<AgCasinoBalance>> GetSummaryBalance(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList)
        {
            const string UspName = "Lrf_Rpt_AGCasinoBalance";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel);
                }
            }
        }

        public async Task<SortableReportResultModel<AgCasinoBalance>> GetCustomerBalanceDetail(
            ThirdPartyWinLossMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            const string UspName = "LRF_Rpt_AGCasinoBalance_Mem";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("custid", metaModel.DrillDownCustomerId);
                db.SetParameterValue("pagesize", metaModel.PageSize);
                db.SetParameterValue("pageindex", metaModel.PageIndex);
                db.SetParameterValue("TotalRows", null);

                UpdateUspParams(db, metaModel);
                SortableReportResultModel<AgCasinoBalance> result;
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    result = GetData(reader, currencyList, metaModel);
                }

                result.TotalRows = Convert.ToInt32(db.GetParameterValue("TotalRows"));

                return result;
            }
        }

        public async Task<SortableReportResultModel<AgCasinoBalance>> GetAgentBalanceDetail(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList)
        {
            const string UspName = "LRF_Rpt_AGCasinoBalance_Ag";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetData(reader, currencyList, metaModel);
                }
            }
        }

        private static void ReadSummaryTotal(IDataReader reader, IReadOnlyDictionary<string, AgCasinoBalance> summaryTotal, string defaultBaseCurrency)
        {
            foreach (var summary in summaryTotal)
            {
                var suffixColumn = !string.IsNullOrEmpty(defaultBaseCurrency) && summary.Key.Equals(defaultBaseCurrency, StringComparison.OrdinalIgnoreCase)
                                     ? "Base"
                                     : summary.Key;

                summary.Value.CustomerWinLoss += reader.ToDecimal("winlost" + suffixColumn);
                summary.Value.TotalTransaction += reader.ToInt("totaltrans");
                summary.Value.LicenseeWL += reader.ToDecimal("licenseewinlost" + suffixColumn);
                summary.Value.TurnOver += reader.ToDecimal("turnover" + suffixColumn);
                summary.Value.OpeningBalance += reader.ToDecimal("OpeningBalance" + suffixColumn);
                summary.Value.TransferIn += reader.ToDecimal("TransferIn" + suffixColumn);
                summary.Value.TransferOut += reader.ToDecimal("TransferOut" + suffixColumn);
                summary.Value.ClosingBalance += reader.ToDecimal("ClosingBalance" + suffixColumn);
            }
        }

        private static void UpdateUspParams(IObjectDb db, ThirdPartyWinLossMetaModel metaModel)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("fromdate", metaModel.FromDate.Date);
            db.SetParameterValue("todate", metaModel.ToDate.Date);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("DefaultBaseCurrency", metaModel.DefaultBaseCurrency);
        }

        private SortableReportResultModel<AgCasinoBalance> GetData(
            IDataReader reader,
            IReadOnlyDictionary<int, string> currencyList,
            ThirdPartyWinLossMetaModel metaModel)
        {
            var dataList = new SortableList<AgCasinoBalance>();

            var summaryTotal = SystemConstants.BaseCurrencyListOfSummaryTotalRow.ToDictionary(currency => currency, _ => new AgCasinoBalance());

            if (!summaryTotal.ContainsKey(metaModel.DefaultBaseCurrency))
            {
                summaryTotal.Add(metaModel.DefaultBaseCurrency, new AgCasinoBalance());
            }

            var hasSystemIdColumn = reader.HasColumn("username");
            var hasExternalIdColumn = reader.HasColumn("nickname");
            var hasCustomerIdColumn = reader.HasColumn("custid");
            var hasWinlostDateColumn = reader.HasColumn("date");

            var index = metaModel.DrillDownLevel == 2 ? ((metaModel.PageIndex - 1) * metaModel.PageSize) + 1 : 1;

            while (reader.Read())
            {
                var currencyId = reader.ToInt("currency");
                var item = new AgCasinoBalance
                {
                    Index = index++,
                    SystemId = hasSystemIdColumn ? reader.ToString("username") : string.Empty,
                    ExternalId = hasExternalIdColumn ? reader.ToString("nickname") : string.Empty,
                    CustomerId = hasCustomerIdColumn ? reader.ToInt("custid") : 0,
                    CurrencyId = currencyId,
                    CurrencyName = currencyList?.GetValueOrDefault(currencyId, currencyId.ToString()),
                    CustomerWinLoss = reader.ToDecimal("winlost"),
                    LicenseeWL = reader.ToDecimal("licenseewinlost"),
                    TurnOver = reader.ToDecimal("turnover"),
                    TotalTransaction = reader.ToInt("totaltrans"),
                    Date = hasWinlostDateColumn ? reader.ToDateTime("date") as DateTime? : metaModel.FromDate,
                    OpeningBalance = reader.ToDecimal("OpeningBalance"),
                    TransferIn = reader.ToDecimal("TransferIn"),
                    TransferOut = reader.ToDecimal("TransferOut"),
                    ClosingBalance = reader.ToDecimal("ClosingBalance"),
                };

                dataList.Add(item);
                ReadSummaryTotal(reader, summaryTotal, metaModel.DefaultBaseCurrency);
            }

            return new SortableReportResultModel<AgCasinoBalance>
            {
                DataList = dataList,
                SummaryTotal = summaryTotal
            };
        }
    }
}
