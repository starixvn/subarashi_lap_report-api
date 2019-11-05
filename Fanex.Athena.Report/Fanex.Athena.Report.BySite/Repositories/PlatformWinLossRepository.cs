namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Resources.Helpers;
    using Fanex.Data;

    public class PlatformWinLossRepository : IPlatformWinLossRepository
    {
        private const int OneHunderPercent = 100;

        private readonly IBalanceUpService _balanceUpService;

        private readonly ICurrencyService _currencyService;

        public PlatformWinLossRepository(IBalanceUpService balanceUpService, ICurrencyService currencyService)
        {
            _balanceUpService = balanceUpService;
            _currencyService = currencyService;
        }

        public async Task<ResultModel<PlatformWinLossSummary>> GetSummaryDataAsync(PlatformWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_PlatformWinloss";

            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return ReadSummaryList(reader);
                }
            }
        }

        public async Task<SortableResultModel<PlatformWinLossMember>> GetMemberDataAsync(PlatformWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_PlatformWinlossDetail";

            var balanceUpInfoTask = _balanceUpService.GetBalanceUpAsync();
            var currencyListTask = _currencyService.GetCurrencyListAsync();
            await Task.WhenAll(balanceUpInfoTask, currencyListTask).ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParamsAsync(db, metaModel, (await balanceUpInfoTask).MoveBetsDay);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return ReadMemberList(reader, await currencyListTask);
                }
            }
        }

        private static ResultModel<PlatformWinLossSummary> ReadSummaryList(IDataReader reader)
        {
            var dataList = new List<PlatformWinLossSummary>();
            var totalData = new PlatformWinLossSummary();

            var parentRows = new Dictionary<int, PlatformWinLossSummary>();
            var childId = -1;
            while (reader.Read())
            {
                var platformType = Convert.ToInt32(reader["PlatformType"]);

                PlatformWinLossSummary parent;
                if (!parentRows.ContainsKey(platformType))
                {
                    parent = new PlatformWinLossSummary
                    {
                        PlatformType = platformType,
                        Name = DataHelper.GetPlatformName(platformType),
                        ProductCode = string.Empty
                    };
                    parentRows.Add(platformType, parent);
                    dataList.Add(parent);
                }
                else
                {
                    parent = parentRows[platformType];
                }

                var child = ReadSummaryItem(reader, childId--, platformType.ToString());

                dataList.Add(child);
                AddTotalSummaryData(parent, child);
                AddTotalSummaryData(totalData, child);
            }

            CalculateTurnoverPercent(dataList, totalData);

            return new ResultModel<PlatformWinLossSummary>
            {
                DataList = dataList,
                TotalData = totalData
            };
        }

        private static PlatformWinLossSummary ReadSummaryItem(IDataReader reader, int id, string parent, byte level = 1, bool isLeaf = true)
        {
            var productCode = reader["productprefix"].ToString().ToUpperInvariant();

            return new PlatformWinLossSummary
            {
                Parent = parent,
                PlatformType = id,
                IsLeaf = isLeaf,
                Level = level,
                Name = DataHelper.GetProductName(productCode),
                TotalTransactions = Convert.ToInt32(reader["Betcount"]),
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                CustomerWinLoss = Convert.ToDecimal(reader["Winlost"]),
                CustomerComm = Convert.ToDecimal(reader["PlayerComm"]),
                LicenseeWL = Convert.ToDecimal(reader["LicenseeWL"]),
                ProductCode = productCode
            };
        }

        private static SortableResultModel<PlatformWinLossMember> ReadMemberList(IDataReader reader, IEnumerable<Currency> currencyList)
        {
            var dataList = new SortableList<PlatformWinLossMember>();
            var totalData = new PlatformWinLossMember();

            var index = 1;
            while (reader.Read())
            {
                var currencyId = Convert.ToInt32(reader["Currency"]);
                var item = new PlatformWinLossMember
                {
                    Index = index,
                    CurrencyName = currencyList.FirstOrDefault(x => x.ID == currencyId)?.Name ?? currencyId.ToString(),
                    TotalTransactions = Convert.ToInt32(reader["Betcount"]),
                    Turnover = Convert.ToDecimal(reader["Turnover"]),
                    CustomerWinLoss = Convert.ToDecimal(reader["Winlost"]),
                    CustomerComm = Convert.ToDecimal(reader["PlayerComm"]),
                    LicenseeWL = Convert.ToDecimal(reader["LicenseeWL"]),
                    BACommission = Convert.ToDecimal(reader["CommBA"]),
                    AmtDueToBA = Convert.ToDecimal(reader["AmtDueToBA"]),
                    CurrencyId = Convert.ToInt32(reader["Currency"]),
                    SystemId = reader["username"].ToString(),
                    CustomerId = Convert.ToInt32(reader["custid"]),
                    ExternalId = reader["nickname"].ToString()
                };

                dataList.Add(item);
                AddTotalMemberData(totalData, item);
                index++;
            }

            return new SortableResultModel<PlatformWinLossMember>
            {
                DataList = dataList,
                TotalData = totalData
            };
        }

        private static void CalculateTurnoverPercent(List<PlatformWinLossSummary> dataList, PlatformWinLossSummary totalData)
        {
            totalData.TurnoverPercent = totalData.Turnover > 0 ? OneHunderPercent : 0;

            if (totalData.Turnover > 0)
            {
                foreach (var item in dataList)
                {
                    item.TurnoverPercent = (item.Turnover / totalData.Turnover) * OneHunderPercent;
                }
            }
        }

        private static void AddTotalSummaryData(PlatformWinLossSummary totalData, PlatformWinLossSummary child)
        {
            totalData.TotalTransactions += child.TotalTransactions;
            totalData.Turnover += child.Turnover;
            totalData.CustomerWinLoss += child.CustomerWinLoss;
            totalData.CustomerComm += child.CustomerComm;
            totalData.LicenseeWL += child.LicenseeWL;
        }

        private static void AddTotalMemberData(PlatformWinLossMember totalData, PlatformWinLossMember item)
        {
            totalData.TotalTransactions += item.TotalTransactions;
            totalData.Turnover += item.Turnover;
            totalData.CustomerWinLoss += item.CustomerWinLoss;
            totalData.CustomerComm += item.CustomerComm;
            totalData.LicenseeWL += item.LicenseeWL;
            totalData.BACommission += item.BACommission;
            totalData.AmtDueToBA += item.AmtDueToBA;
        }

        private static void UpdateUspParamsAsync(IObjectDb db, PlatformWinLossMetaModel metaModel, DateTime moveBetsDay)
        {
            db.SetParameterValue("PlatformType", metaModel.PlatformType);
            db.SetParameterValue("movebetsday", moveBetsDay);
            db.SetParameterValue("DrawFilter", metaModel.DrawFilter);
            db.SetParameterValue("FromDate", metaModel.FromDate);
            db.SetParameterValue("ToDate", metaModel.ToDate);
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);

            foreach (var product in metaModel.ProductOptions.Where(x => x.Status))
            {
                db.SetParameterValue("show" + product.Code, true);
            }
        }
    }
}
