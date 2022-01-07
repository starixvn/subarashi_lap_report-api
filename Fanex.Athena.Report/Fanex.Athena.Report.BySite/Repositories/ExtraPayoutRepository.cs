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
    using Fanex.Athena.Report.Common.Model.DataDomain;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Helpers;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Athena.Report.Resources.Helpers;
    using Fanex.Data;

    public class ExtraPayoutRepository : IExtraPayoutRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly IDictionary<int, int> _extraPayoutMapType = new Dictionary<int, int>
        {
            { ExtraPayoutMapType.PPTournamentPrize, BetType.PPTournamentPrize },
            { ExtraPayoutMapType.PPPromotionPrize, BetType.PPPromotionPrize },
            { ExtraPayoutMapType.PPJackpotPrize, BetType.PPJackpotPrize },
            { ExtraPayoutMapType.PPJackpotContribution, BetType.PPJackpotContribution },
            { ExtraPayoutMapType.SabaLuckySpin, BetType.SabaLuckySpin },
            { ExtraPayoutMapType.SabaLuckySpot, BetType.SabaLuckySpot },
            { ExtraPayoutMapType.SabaNonCashPrize, BetType.SabaNonCashPrize },
            { ExtraPayoutMapType.SabaLoyaltyPointsExchange, BetType.SabaLoyaltyPointsExchange },
            { ExtraPayoutMapType.SabaRedEnvelope, BetType.SabaRedEnvelope },
            { ExtraPayoutMapType.SAGamingTip, BetType.SAGamingTip },
            { ExtraPayoutMapType.SCBFreeBets, BetType.SCBFreeBets },
            { ExtraPayoutMapType.SCBFreeBetsJackpotPrize, BetType.SCBFreeBetsJackpotPrize },
            { ExtraPayoutMapType.SCBFreeBetsJackpotContribution, BetType.SCBFreeBetsJackpotContribution },
            { ExtraPayoutMapType.EASexyLuckyDraw, BetType.EASexyLuckyDraw },
            { ExtraPayoutMapType.ArcadiaGamingBonus, BetType.ArcadiaGamingBonus },
            { ExtraPayoutMapType.VgamingFreeBets, BetType.VgamingFreeBets },
            { ExtraPayoutMapType.VgamingFreeBetsJackpotPrize, BetType.VgamingFreeBetsJackpotPrize },
            { ExtraPayoutMapType.VgamingFreeBetsJackpotContribution, BetType.VgamingFreeBetsJackpotContribution }
        };

        public ExtraPayoutRepository(
            IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMainData(
            ExtraPayoutMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_ExtraPayout";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("ReportType", metaModel.ReportBy);
                db.SetParameterValue("Username", metaModel.Username);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetMainData(reader, metaModel);
                }
            }
        }

        private static void ReadCommonData(IDataReader reader, BaseExtraPayout data, ExtraPayoutMetaModel metaModel)
        {
            var productCode = reader.GetVal<string>("ProductCode");
            var currency = reader.GetVal<int>("Currency");

            data.BetCount = reader.GetVal<int>("betcount");
            data.ProductCode = productCode;
            data.ProductName = !string.IsNullOrEmpty(productCode) ? DataHelper.GetProductName(productCode) : string.Empty;
            data.BasedTurnover = reader.GetVal<decimal>("BasedTurnover");
            data.BasedAmount = reader.GetVal<decimal>("BasedAmount");
            data.BasedSharingAmount = reader.GetVal<decimal>("BasedSharingAmount");
            data.Turnover = metaModel.IsConvertAllToBaseCurrency ? data.BasedTurnover : reader.GetVal<decimal>("Turnover");
            data.Amount = metaModel.IsConvertAllToBaseCurrency ? data.BasedAmount : reader.GetVal<decimal>("Amount");
            data.SharingAmount = metaModel.IsConvertAllToBaseCurrency ? data.BasedSharingAmount : reader.GetVal<decimal>("SharingAmount");
            data.Currency = currency;
            data.CurrencyName = metaModel.UserCurrencyList.TryGetValue(currency, out string currencyName) ? currencyName : string.Empty;
        }

        private static IEnumerable<ExtraPayout> GetSumDataList(IEnumerable<ExtraPayout> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new List<ExtraPayout>();
            }

            return dataList.GroupBy(d => d.Currency).Select(g => new ExtraPayout
            {
                Currency = g.FirstOrDefault().Currency,
                CurrencyName = g.FirstOrDefault().CurrencyName,
                BetCount = g.Sum(_ => _.BetCount),
                Turnover = g.Sum(_ => _.BasedTurnover),
                Amount = g.Sum(_ => _.BasedAmount),
                SharingAmount = g.Sum(_ => _.BasedSharingAmount)
            }).ToList();
        }

        private static ExtraPayout GetSumTotalData(IEnumerable<ExtraPayout> dataList, string baseCurrency)
        {
            if (dataList == null || !dataList.Any())
            {
                return new ExtraPayout();
            }

            return new ExtraPayout
            {
                CurrencyName = baseCurrency,
                BetCount = dataList.Sum(_ => _.BetCount),
                Turnover = dataList.Sum(_ => _.BasedTurnover),
                Amount = dataList.Sum(_ => _.BasedAmount),
                SharingAmount = dataList.Sum(_ => _.BasedSharingAmount)
            };
        }

        private static void UpdateSpCommonParamsAsync(IObjectDb db, ExtraPayoutMetaModel metaModel, DateTime moveBetsDay)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency.ToUpperInvariant());
            db.SetParameterValue("fromdate", metaModel.DrillDownLevel == (int)ReportLevel.Main ? metaModel.FromDate.Date : metaModel.DrillDownFromDate.Date);
            db.SetParameterValue("todate", metaModel.DrillDownLevel == (int)ReportLevel.Main ? metaModel.ToDate.Date : metaModel.DrillDownToDate.Date);
            db.SetParameterValue("productList", metaModel.ProductCodeList);
            db.SetParameterValue("PromotionTypeList", metaModel.PromotionTypeIdList);
            db.SetParameterValue("movebetsday", moveBetsDay);
        }

        private SortableReportResultModel<ExtraPayout> GetMainData(
            IDataReader reader,
            ExtraPayoutMetaModel metaModel)
        {
            var dataList = new SortableList<ExtraPayout>();
            var isMonthly = metaModel.ReportBy == ReportByType.Monthly;

            while (reader.Read())
            {
                var hasErrCode = reader.HasColumn("ErrCode");
                if (hasErrCode)
                {
                    return new SortableReportResultModel<ExtraPayout>
                    {
                        DataList = new SortableList<ExtraPayout>()
                    };
                }

                var date = ConvertHelper.ToDateTime(reader.GetVal<string>("reporttype"), isMonthly);
                var dateString = isMonthly ? date.ToString("MM/yyyy") : date.ToString("MM/dd/yyyy");

                var item = new ExtraPayout
                {
                    Date = date,
                    DateString = dateString
                };
                ReadCommonData(reader, item, metaModel);

                dataList.Add(item);
            }

            var groupData = DoRestructureData(dataList.GroupBy(i => new { i.Date }));

            return new SortableReportResultModel<ExtraPayout>
            {
                DataList = new SortableList<ExtraPayout>(groupData),
                TotalDataList = GetSumDataList(groupData),
                TotalData = GetSumTotalData(groupData, metaModel.BaseCurrency.ToUpperInvariant()),
                TotalRows = groupData.Count()
            };
        }

        public async Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMemberData(
            ExtraPayoutMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_ExtraPayout_Mem";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("Username", metaModel.Username);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetMemberData(reader, metaModel);
                }
            }
        }

        private SortableReportResultModel<ExtraPayout> GetMemberData(
            IDataReader reader,
            ExtraPayoutMetaModel metaModel)
        {
            var dataList = new SortableList<ExtraPayout>();
            var isMonthly = metaModel.ReportBy == ReportByType.Monthly;
            var dateString = isMonthly ? metaModel.FromDate.ToString("MM/yyyy") : metaModel.FromDate.ToString("MM/dd/yyyy");

            while (reader.Read())
            {
                var hasErrCode = reader.HasColumn("ErrCode");
                if (hasErrCode)
                {
                    return new SortableReportResultModel<ExtraPayout>
                    {
                        DataList = new SortableList<ExtraPayout>()
                    };
                }

                var item = new ExtraPayout
                {
                    Date = metaModel.FromDate.Date,
                    DateString = dateString
                };
                ReadCommonData(reader, item, metaModel);
                ReadMemberData(reader, item);

                dataList.Add(item);
            }

            if (metaModel.IsExportExcel)
            {
                return new SortableReportResultModel<ExtraPayout>
                {
                    DataList = new SortableList<ExtraPayout>(dataList),
                    TotalDataList = GetSumDataList(dataList),
                    TotalData = GetSumTotalData(dataList, metaModel.BaseCurrency.ToUpperInvariant()),
                    TotalRows = dataList.Count()
                };
            }

            var beginIndex = (metaModel.PageIndex - 1) * metaModel.PageSize;
            var rawGroupPagingData = dataList.GroupBy(i => new { i.CustId });

            var groupData = DoRestructureData(rawGroupPagingData.Skip(beginIndex).Take(metaModel.PageSize), beginIndex);

            return new SortableReportResultModel<ExtraPayout>
            {
                DataList = new SortableList<ExtraPayout>(groupData),
                TotalDataList = GetSumDataList(groupData),
                TotalData = GetSumTotalData(groupData, metaModel.BaseCurrency.ToUpperInvariant()),
                TotalRows = rawGroupPagingData.Count()
            };
        }

        private void ReadMemberData(IDataReader reader, ExtraPayout data)
        {
            var promotionType = reader.GetVal<int>("PromotionType");

            data.CustId = reader.GetVal<int>("CustId");
            data.Username = reader.GetVal<string>("username");
            data.PromotionType = promotionType;
            data.PromotionName = GetPromotionName(promotionType);
            data.Nickname = reader.GetVal<string>("Nickname");
        }

        public async Task<SortableReportResultModel<ExtraPayoutDetail>> GetExtraPayoutDetailData(
            ExtraPayoutDetailMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_ExtraPayout_Mem_Detail";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("CustId", metaModel.CustId);
                db.SetParameterValue("BalanceUpDay", balanceUpInfo.BalanceUpDay);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetDetailData(reader, metaModel);
                }
            }
        }

        private SortableReportResultModel<ExtraPayoutDetail> GetDetailData(
            IDataReader reader, ExtraPayoutDetailMetaModel metaModel)
        {
            var dataList = new SortableList<ExtraPayoutDetail>();

            while (reader.Read())
            {
                var productCode = reader.GetVal<string>("ProductCode");
                var promotionType = reader.GetVal<int>("PromotionType");
                var date = reader.GetVal<DateTime>("WinLostDate");
                var dateString = date.ToString("MM/dd/yyyy");

                var item = new ExtraPayoutDetail
                {
                    Date = date,
                    DateString = dateString,
                    ProductCode = productCode,
                    ProductName = DataHelper.GetProductName(productCode),
                    PromotionType = promotionType,
                    PromotionName = GetPromotionName(promotionType),
                    BasedTurnover = reader.GetVal<decimal>("BasedTurnover"),
                    BasedAmount = reader.GetVal<decimal>("BasedAmount"),
                    BasedSharingAmount = reader.GetVal<decimal>("BasedSharingAmount"),
                    Refno = reader.GetVal<string>("RefNo")
                };

                item.Turnover = metaModel.IsConvertAllToBaseCurrency ? item.BasedTurnover : reader.GetVal<decimal>("Turnover");
                item.Amount = metaModel.IsConvertAllToBaseCurrency ? item.BasedAmount : reader.GetVal<decimal>("Amount");
                item.SharingAmount = metaModel.IsConvertAllToBaseCurrency ? item.BasedSharingAmount : reader.GetVal<decimal>("SharingAmount");

                dataList.Add(item);
            }

            return new SortableReportResultModel<ExtraPayoutDetail>
            {
                DataList = dataList,
                TotalData = GetSumDetailData(dataList)
            };
        }

        private static ExtraPayoutDetail GetSumDetailData(List<ExtraPayoutDetail> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new ExtraPayoutDetail();
            }

            return new ExtraPayoutDetail
            {
                Turnover = dataList.Sum(_ => _.BasedTurnover),
                Amount = dataList.Sum(_ => _.BasedAmount),
                SharingAmount = dataList.Sum(_ => _.BasedSharingAmount)
            };
        }

        private static IList<ExtraPayout> DoRestructureData(dynamic groupData, int index = 0)
        {
            var newDataList = new List<ExtraPayout>();

            foreach (var item in groupData)
            {
                var rowSpanFlag = true;
                var temps = (IList<ExtraPayout>)item;
                var numOfItem = temps.Count();

                var curencyRowSpanFlag = true;
                var currency = temps[0].Currency;

                var productRowSpanFlag = true;
                var product = temps[0].ProductCode;

                foreach (var col in temps)
                {
                    if (rowSpanFlag && numOfItem >= 1)
                    {
                        index++;
                        col.IndexRowSpan = numOfItem;
                        rowSpanFlag = false;
                    }
                    else
                    {
                        col.IndexRowSpan = -1;
                    }

                    if (productRowSpanFlag || col.Currency != currency || col.ProductCode != product)
                    {
                        col.ProductRowSpan = temps.Count(_ => _.Currency == col.Currency && _.ProductCode == col.ProductCode);
                        product = col.ProductCode;
                        productRowSpanFlag = false;
                    }
                    else
                    {
                        col.ProductRowSpan = -1;
                    }

                    if (curencyRowSpanFlag || col.Currency != currency)
                    {
                        col.CurencyRowSpan = temps.Count(_ => _.Currency == col.Currency);
                        currency = col.Currency;
                        curencyRowSpanFlag = false;
                    }
                    else
                    {
                        col.CurencyRowSpan = -1;
                    }

                    col.Index = index;
                    newDataList.Add(col);
                }
            }

            return newDataList;
        }

        private static bool IsSportType(int betType)
        {
            return betType == BetType.VgamingFreeBets || betType == BetType.SCBFreeBets;
        }

        public string GetPromotionName(int promotionType)
        {
            promotionType = _extraPayoutMapType.TryGetValue(promotionType, out int mapExtraPayoutType) ? mapExtraPayoutType : promotionType;

            return IsSportType(promotionType) ? _cacheRefAdapter.GetSportName(promotionType) : _cacheRefAdapter.GetBettype(promotionType);
        }
    }
}