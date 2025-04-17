namespace Fanex.Athena.Report.BySite.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Data;
    using DTOs;
    using Interfaces;
    using MetaModels;

    public class WinLossByTimeZoneRepository : IWinLossByTimeZoneRepository
    {
        private readonly IBalanceUpService _balanceUpService;

        public WinLossByTimeZoneRepository(IBalanceUpService balanceUpService)
        {
            _balanceUpService = balanceUpService;
        }

        public async Task<ReportResultModel<WinLossByTimeZone>> GetData(
            WinLossByTimeZoneMetaModel metaModel,
            IReadOnlyDictionary<int, string> currencyList)
        {
            const string SpName = "Lrf_Rpt_LicWinlossByTimeZone";
            var dataList = new SortableList<WinLossByTimeZone>();
            var totalData = new WinLossByTimeZone();

            using (var objectDb = ObjectDbFactory.CreateInstance(SpName))
            {
                await AssignParameter(objectDb, metaModel);

                using (var reader = await objectDb.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        var item = ReadDataInBaseCurrency(reader, currencyList, metaModel);
                        totalData.BetCount += item.BetCount;
                        totalData.Turnover += item.Turnover;
                        totalData.CustomerComm += item.CustomerComm;
                        totalData.LicenseeWinLoss += item.LicenseeWinLoss;
                        totalData.LicenseeComm += item.LicenseeComm;

                        if (!metaModel.IsConvertAll)
                        {
                            item.Turnover = reader.GetVal<decimal>("Turnover");
                            item.CustomerComm = reader.GetVal<decimal>("PlayerComm");
                            item.LicenseeWinLoss = reader.GetVal<decimal>("LicenseeWL");
                            item.LicenseeComm = reader.GetVal<decimal>("LicenseeComm");
                        }

                        dataList.Add(item);
                    }
                }
            }

            return new ReportResultModel<WinLossByTimeZone>
            {
                DataList = dataList,
                TotalData = totalData
            };
        }

        private static WinLossByTimeZone ReadDataInBaseCurrency(
            IDataReader reader,
            IReadOnlyDictionary<int, string> currencyList,
            WinLossByTimeZoneMetaModel metaModel)
        {
            const byte GroupByProduct = 1;
            const int ReportBySummary = 1;
            const int ReportByMonthly = 3;

            var item = new WinLossByTimeZone
            {
                GroupBy = reader.GetVal<string>("GroupBy"),
                BetCount = reader.GetVal<int>("BetCount"),
                Turnover = reader.GetVal<decimal>("TurnoverBase"),
                CustomerComm = reader.GetVal<decimal>("PlayerCommBase"),
                LicenseeWinLoss = reader.GetVal<decimal>("LicenseeWLBase"),
                LicenseeComm = reader.GetVal<decimal>("LicenseeCommBase")
            };

            if (metaModel.ReportBy != ReportBySummary)
            {
                item.ReportBy =
                    ConvertHelper.ToDateTime(reader.GetVal<string>("reportby"), metaModel.ReportBy == ReportByMonthly);
            }

            item.GroupBy = metaModel.GroupBy == GroupByProduct
                ? ProductHelper.GetProductName(item.GroupBy)
                : currencyList?.GetValueOrDefault(reader.GetVal<int>("GroupBy"), item.GroupBy);

            return item;
        }

        private async Task AssignParameter(IObjectDb objectDb, WinLossByTimeZoneMetaModel metaModel)
        {
            objectDb.SetParameterValue("FromDate", metaModel.FromDate);
            objectDb.SetParameterValue("ToDate", metaModel.ToDate);
            objectDb.SetParameterValue("ProductList", metaModel.ProductCodeList);
            objectDb.SetParameterValue("AgentList", metaModel.AgentListXml);
            objectDb.SetParameterValue("TimeZone", metaModel.TimeZone);
            objectDb.SetParameterValue("ReportBy", metaModel.ReportBy);
            objectDb.SetParameterValue("GroupBy", metaModel.GroupBy);
            objectDb.SetParameterValue("IncludeDraw", metaModel.IsIncludeDrawData);
            objectDb.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);

            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            objectDb.SetParameterValue("MoveBetsDay", balanceUp.MoveBetsDay);
        }
    }
}