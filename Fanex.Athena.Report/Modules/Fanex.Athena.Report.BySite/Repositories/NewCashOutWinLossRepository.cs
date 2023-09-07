namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Helpers;
    using Fanex.Athena.Report.Core.Services;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Data;

    public class NewCashOutWinLossRepository : INewCashOutWinLossRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly IContextService _contextService;

        public NewCashOutWinLossRepository(
            IBalanceUpService balanceUpService,
            IContextService contextService)
        {
            _balanceUpService = balanceUpService;
            _contextService = contextService;
        }

        public async Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMainData(
            NewCashOutWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_CashOut";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("ReportType", metaModel.ReportBy);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetMainData(reader, metaModel);
                }
            }
        }

        private void ReadCommonData(IDataReader reader, CashOutWinLoss data, NewCashOutWinLossMetaModel metaModel)
        {
            var currency = reader.GetVal<int>("Currency");

            data.TotalTransaction = reader.GetVal<int>("betcount");
            data.BasedTurnover = reader.GetVal<decimal>("BasedTurnover");
            data.BasedCustomerWL = reader.GetVal<decimal>("BasedWinLost");
            data.BasedLicenseeWL = reader.GetVal<decimal>("BasedLicenseeWL");
            data.BasedCustomerComm = reader.GetVal<decimal>("BasedPlayerComm");
            data.BasedBAComm = reader.GetVal<decimal>("BasedBAComm");
            data.BasedAmtDueToBA = reader.GetVal<decimal>("BasedAmtDuetoBA");
            data.Turnover = metaModel.IsConvertAllToBaseCurrency ? data.BasedTurnover : reader.GetVal<decimal>("Turnover");
            data.CustomerWL = metaModel.IsConvertAllToBaseCurrency ? data.BasedCustomerWL : reader.GetVal<decimal>("WinLost");
            data.LicenseeWL = metaModel.IsConvertAllToBaseCurrency ? data.BasedLicenseeWL : reader.GetVal<decimal>("LicenseeWL");
            data.CustomerComm = metaModel.IsConvertAllToBaseCurrency ? data.BasedCustomerComm : reader.GetVal<decimal>("PlayerComm");
            data.BAComm = metaModel.IsConvertAllToBaseCurrency ? data.BasedBAComm : reader.GetVal<decimal>("BAComm");
            data.AmtDueToBA = metaModel.IsConvertAllToBaseCurrency ? data.BasedAmtDueToBA : reader.GetVal<decimal>("AmtDuetoBA");
            data.Currency = currency;
            data.CurrencyName = _contextService.CurrencyList.TryGetValue(currency, out string currencyName) ? currencyName : string.Empty;
        }

        private static IEnumerable<CashOutWinLoss> GetSumDataList(IEnumerable<CashOutWinLoss> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new List<CashOutWinLoss>();
            }

            return dataList.GroupBy(d => d.Currency).Select(g => new CashOutWinLoss
            {
                Currency = g.FirstOrDefault()?.Currency ?? default,
                CurrencyName = g.FirstOrDefault()?.CurrencyName,
                TotalTransaction = g.Sum(_ => _.TotalTransaction),
                Turnover = g.Sum(_ => _.BasedTurnover),
                CustomerWL = g.Sum(_ => _.BasedCustomerWL),
                LicenseeWL = g.Sum(_ => _.BasedLicenseeWL),
                CustomerComm = g.Sum(_ => _.BasedCustomerComm),
                BAComm = g.Sum(_ => _.BasedBAComm),
                AmtDueToBA = g.Sum(_ => _.BasedAmtDueToBA)
            }).ToList();
        }

        private static CashOutWinLoss GetSumTotalData(IEnumerable<CashOutWinLoss> dataList, string baseCurrency)
        {
            if (dataList == null || !dataList.Any())
            {
                return new CashOutWinLoss();
            }

            return new CashOutWinLoss
            {
                CurrencyName = baseCurrency,
                TotalTransaction = dataList.Sum(_ => _.TotalTransaction),
                Turnover = dataList.Sum(_ => _.BasedTurnover),
                CustomerWL = dataList.Sum(_ => _.BasedCustomerWL),
                LicenseeWL = dataList.Sum(_ => _.BasedLicenseeWL),
                CustomerComm = dataList.Sum(_ => _.BasedCustomerComm),
                BAComm = dataList.Sum(_ => _.BasedBAComm),
                AmtDueToBA = dataList.Sum(_ => _.BasedAmtDueToBA)
            };
        }

        private static void UpdateSpCommonParamsAsync(IObjectDb db, NewCashOutWinLossMetaModel metaModel, DateTime moveBetsDay)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("SportTypeList", metaModel.SportIdList);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency.ToUpperInvariant());
            db.SetParameterValue("fromdate", metaModel.DrillDownLevel == (int)ReportLevel.Main ? metaModel.FromDate.Date : metaModel.DrillDownFromDate.Date);
            db.SetParameterValue("todate", metaModel.DrillDownLevel == (int)ReportLevel.Main ? metaModel.ToDate.Date : metaModel.DrillDownToDate.Date);
            db.SetParameterValue("movebetsday", moveBetsDay);
        }

        private SortableReportResultModel<CashOutWinLoss> GetMainData(
            IDataReader reader,
            NewCashOutWinLossMetaModel metaModel)
        {
            var dataList = new SortableList<CashOutWinLoss>();
            var isMonthly = metaModel.ReportBy == ReportByType.Monthly;
            var index = 1;

            while (reader.Read())
            {
                var hasErrCode = reader.HasColumn("ErrCode");
                if (hasErrCode)
                {
                    return new SortableReportResultModel<CashOutWinLoss>
                    {
                        DataList = new SortableList<CashOutWinLoss>()
                    };
                }

                var date = ConvertHelper.ToDateTime(reader.GetVal<string>("reporttype"), isMonthly);
                var dateString = isMonthly ? date.ToString("MM/yyyy") : date.ToString("MM/dd/yyyy");

                var item = new CashOutWinLoss
                {
                    Index = index,
                    Date = date,
                    DateString = dateString
                };
                ReadCommonData(reader, item, metaModel);

                dataList.Add(item);
                index++;
            }

            SortData(dataList, metaModel.SortBy, metaModel.SortOrder);

            return new SortableReportResultModel<CashOutWinLoss>
            {
                DataList = new SortableList<CashOutWinLoss>(dataList),
                TotalDataList = GetSumDataList(dataList),
                TotalData = GetSumTotalData(dataList, metaModel.BaseCurrency.ToUpperInvariant()),
                TotalRows = dataList.Count()
            };
        }

        public async Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMemberData(
            NewCashOutWinLossMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_CashOut_Mem";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetMemberData(reader, metaModel);
                }
            }
        }

        private SortableReportResultModel<CashOutWinLoss> GetMemberData(
            IDataReader reader,
            NewCashOutWinLossMetaModel metaModel)
        {
            var dataList = new SortableList<CashOutWinLoss>();
            var isMonthly = metaModel.ReportBy == ReportByType.Monthly;
            var dateString = isMonthly ? metaModel.FromDate.ToString("MM/yyyy") : metaModel.FromDate.ToString("MM/dd/yyyy");
            var beginIndex = (metaModel.PageIndex - 1) * metaModel.PageSize;
            var index = beginIndex;

            while (reader.Read())
            {
                index++;
                var hasErrCode = reader.HasColumn("ErrCode");
                if (hasErrCode)
                {
                    return new SortableReportResultModel<CashOutWinLoss>
                    {
                        DataList = new SortableList<CashOutWinLoss>()
                    };
                }

                var item = new CashOutWinLoss
                {
                    Index = index,
                    Date = metaModel.FromDate.Date,
                    DateString = dateString
                };
                ReadCommonData(reader, item, metaModel);
                ReadMemberData(reader, item);

                dataList.Add(item);
            }

            SortData(dataList, metaModel.SortBy, metaModel.SortOrder);

            if (metaModel.IsExportExcel)
            {
                return new SortableReportResultModel<CashOutWinLoss>
                {
                    DataList = new SortableList<CashOutWinLoss>(dataList),
                    TotalDataList = GetSumDataList(dataList),
                    TotalData = GetSumTotalData(dataList, metaModel.BaseCurrency.ToUpperInvariant()),
                    TotalRows = dataList.Count()
                };
            }

            var pagingData = dataList.Skip(beginIndex).Take(metaModel.PageSize);

            return new SortableReportResultModel<CashOutWinLoss>
            {
                DataList = new SortableList<CashOutWinLoss>(pagingData),
                TotalDataList = GetSumDataList(pagingData),
                TotalData = GetSumTotalData(pagingData, metaModel.BaseCurrency.ToUpperInvariant()),
                TotalRows = dataList.Count()
            };
        }

        private static void ReadMemberData(IDataReader reader, CashOutWinLoss data)
        {
            data.CustId = reader.GetVal<int>("CustId");
            data.SystemId = reader.GetVal<string>("username");
            data.ExternalId = reader.GetVal<string>("Nickname");
        }

        private static void SortData(SortableList<CashOutWinLoss> dataList, string sortBy, string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                var property = typeof(CashOutWinLoss).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    dataList.Sort(property.Name, sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase), nameof(CashOutWinLoss.Index), true);
                }
            }
        }
    }
}