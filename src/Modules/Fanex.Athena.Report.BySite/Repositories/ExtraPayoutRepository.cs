namespace Fanex.Athena.Report.BySite.Repositories
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Helpers;
    using Fanex.Athena.Report.Common.Model.DataDomain;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using Fanex.Athena.Report.Resources.Helpers;
    using Fanex.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    public class ExtraPayoutRepository : IExtraPayoutRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IContextService _contextService;

        private static readonly IDictionary<int, int> _extraPayoutMapType = new Dictionary<int, int>
        {
            { ExtraPayoutMapType.PayoutPPTournamentPrize, BetType.PPTournamentPrize },
            { ExtraPayoutMapType.PayoutPPPromotionPrize, BetType.PPPromotionPrize },
            { ExtraPayoutMapType.PayoutPPJackpotPrize, BetType.PPJackpotPrize },
            { ExtraPayoutMapType.PayoutPPJackpotContribution, BetType.PPJackpotContribution },
            { ExtraPayoutMapType.PayoutSabaLuckySpin, BetType.SabaLuckySpin },
            { ExtraPayoutMapType.PayoutSabaLuckySpot, BetType.SabaLuckySpot },
            { ExtraPayoutMapType.PayoutSabaNonCashPrize, BetType.SabaNonCashPrize },
            { ExtraPayoutMapType.PayoutSabaLoyaltyPointsExchange, BetType.SabaLoyaltyPointsExchange },
            { ExtraPayoutMapType.PayoutSabaRedEnvelope, BetType.SabaRedEnvelope },
            { ExtraPayoutMapType.PayoutSAGamingTip, BetType.SAGamingTip },
            { ExtraPayoutMapType.PayoutSCBFreeBets, BetType.SCBFreeBets },
            { ExtraPayoutMapType.PayoutSCBFreeBetsJackpotPrize, BetType.SCBFreeBetsJackpotPrize },
            { ExtraPayoutMapType.PayoutSCBFreeBetsJackpotContribution, BetType.SCBFreeBetsJackpotContribution },
            { ExtraPayoutMapType.PayoutEASexyLuckyDraw, BetType.EASexyLuckyDraw },
            { ExtraPayoutMapType.PayoutArcadiaGamingBonus, BetType.ArcadiaGamingBonus },
            { ExtraPayoutMapType.PayoutVgamingFreeBets, BetType.VgamingFreeBets },
            { ExtraPayoutMapType.PayoutVgamingPromotionPrize, BetType.VgamingPromotionPrize },
            { ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotPrize, BetType.VgamingFreeBetsJackpotPrize },
            { ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotContribution, BetType.VgamingFreeBetsJackpotContribution },
            { ExtraPayoutMapType.PayoutSGJackpotPrize, BetType.SGJackpotPrize },
            { ExtraPayoutMapType.PayoutSGJackpotContribution, BetType.SGJackpotContribution },
            { ExtraPayoutMapType.PayoutSGPromotion, BetType.SGPromotion },
            { ExtraPayoutMapType.PayoutAllbetPromotion, BetType.AllbetPromotion },
            { ExtraPayoutMapType.PayoutStreamerTips, BetType.TIPStreamerTips },
            { ExtraPayoutMapType.PayoutMGRNGPromotion, BetType.MGRNGPromotion },
            { ExtraPayoutMapType.PayoutSabaCasinoRoyaleTournamentPrize, BetType.SabaCasinoRoyaleTournamentPrize },
            { ExtraPayoutMapType.PayoutSabaRewardRiskFreeRebate, BetType.SabaRewardRiskFreeRebate },
            { ExtraPayoutMapType.Live22Promotion, BetType.Live22Promotion },
            { ExtraPayoutMapType.PayoutSabaCoinsCashExchange, BetType.SabaCoinsCashExchange },
            { ExtraPayoutMapType.PayoutPromotionJackpotPrize, BetType.PromotionJackpotPrize },
            { ExtraPayoutMapType.PayoutPromotionCashPrize, BetType.PromotionCashPrize },
            { ExtraPayoutMapType.PayoutFastSpinPromotion, BetType.FastSpinPromotionPrize },
            { ExtraPayoutMapType.PayoutFACHAIPromotion, BetType.FACHAIPromotion },
            { ExtraPayoutMapType.PayoutOddsBoostAdjustment, BetType.OddsBoostAdjustment }
        };

        private static readonly int[] SBPromotionBettypes = new[] {
            BetType.SabaLuckySpin,
            BetType.SabaLuckySpot,
            BetType.SabaNonCashPrize,
            BetType.SabaLoyaltyPointsExchange,
            BetType.SabaRedEnvelope,
            BetType.SabaRewardRiskFreeRebate,
            BetType.SabaCoinsCashExchange,
            BetType.PromotionJackpotPrize,
            BetType.PromotionCashPrize
        };

        private static readonly int[] CasinoPromotionBettypes = new[] {
            BetType.SabaCasinoRoyaleTournamentPrize
        };

        public ExtraPayoutRepository(
            IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter, IContextService contextService)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
            _contextService = contextService;
        }

        public async Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMainData(
            ExtraPayoutMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_ExtraPayout";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("ReportType", metaModel.ReportBy);
                db.SetParameterValue("Username", metaModel.Username);
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo.MoveBetsDay);

                using (var reader = await db.ExecuteReaderAsync())
                {
                    return GetMainData(reader, metaModel);
                }
            }
        }

        private void ReadCommonData(IDataReader reader, BaseExtraPayout data, ExtraPayoutMetaModel metaModel)
        {
            var productCode = reader.GetVal<string>("ProductCode");
            var currency = reader.GetVal<int>("Currency");
            var basedSharingAmount = reader.GetVal<decimal>("BasedSharingAmount");
            var sharingAmount = metaModel.IsConvertAllToBaseCurrency ? basedSharingAmount : reader.GetVal<decimal>("SharingAmount");

            data.BetCount = reader.GetVal<int>("betcount");
            data.ProductCode = productCode;
            data.ProductName = !string.IsNullOrEmpty(productCode) ? DataHelper.GetProductName(productCode) : string.Empty;
            data.BasedTurnover = reader.GetVal<decimal>("BasedTurnover");
            data.BasedAmount = reader.GetVal<decimal>("BasedAmount");
            data.BasedSharingAmount = productCode == Product.TIP ? 0 : basedSharingAmount;
            data.Turnover = metaModel.IsConvertAllToBaseCurrency ? data.BasedTurnover : reader.GetVal<decimal>("Turnover");
            data.Amount = metaModel.IsConvertAllToBaseCurrency ? data.BasedAmount : reader.GetVal<decimal>("Amount");
            data.SharingAmount = productCode == Product.TIP ? 0 : sharingAmount;
            data.Currency = currency;
            data.CurrencyName = _contextService.CurrencyList.TryGetValue(currency, out string currencyName) ? currencyName : string.Empty;
        }

        private static IEnumerable<ExtraPayout> GetSumDataList(IEnumerable<ExtraPayout> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new List<ExtraPayout>();
            }

            return dataList.GroupBy(d => d.Currency).Select(g => new ExtraPayout
            {
                Currency = g.FirstOrDefault()?.Currency ?? default,
                CurrencyName = g.FirstOrDefault()?.CurrencyName,
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
                ReadMemberData(reader, item, metaModel.IsHideSaba);

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

        private void ReadMemberData(IDataReader reader, ExtraPayout data, bool isHideSaba)
        {
            var promotionType = reader.GetVal<int>("PromotionType");

            data.CustId = reader.GetVal<int>("CustId");
            data.Username = GetUserNameValue(reader, promotionType);
            data.PromotionType = promotionType;
            data.PromotionName = GetPromotionName(promotionType, isHideSaba);
            data.Nickname = GetNicknameValue(reader, promotionType);
        }

        private static string GetUserNameValue(IDataReader reader, int promotionType)
        {
            return promotionType == ExtraPayoutMapType.PayoutOddsBoostAdjustment ?
                ViewResources.TotalOddsBootsBonus :
                reader.GetVal<string>("username");
        }

        private static string GetNicknameValue(IDataReader reader, int promotionType)
        {
            return promotionType == ExtraPayoutMapType.PayoutOddsBoostAdjustment ?
                string.Empty :
                reader.GetVal<string>("Nickname");
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
                var basedSharingAmount = reader.GetVal<decimal>("BasedSharingAmount");
                var sharingAmount = metaModel.IsConvertAllToBaseCurrency ? basedSharingAmount : reader.GetVal<decimal>("SharingAmount");
                var betID = promotionType == ExtraPayoutMapType.PayoutSabaRewardRiskFreeRebate ? reader.GetVal<string>("BetID") : "-";

                var item = new ExtraPayoutDetail
                {
                    Date = date,
                    DateString = dateString,
                    ProductCode = productCode,
                    ProductName = DataHelper.GetProductName(productCode),
                    PromotionType = promotionType,
                    PromotionName = GetPromotionName(promotionType, metaModel.IsHideSaba),
                    BasedTurnover = reader.GetVal<decimal>("BasedTurnover"),
                    BasedAmount = reader.GetVal<decimal>("BasedAmount"),
                    BasedSharingAmount = productCode == Product.TIP ? 0 : basedSharingAmount,
                    Refno = reader.GetVal<string>("RefNo"),
                    BetID = betID
                };

                item.Turnover = metaModel.IsConvertAllToBaseCurrency ? item.BasedTurnover : reader.GetVal<decimal>("Turnover");
                item.Amount = metaModel.IsConvertAllToBaseCurrency ? item.BasedAmount : reader.GetVal<decimal>("Amount");
                item.SharingAmount = productCode == Product.TIP ? 0 : sharingAmount;

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
                    CalculateIndexRowSpan(col, numOfItem, ref rowSpanFlag, ref index);

                    CalculateProductRowSpan(temps, col, ref productRowSpanFlag, currency, ref product);

                    CalculateCurrencyRowSpan(temps, col, ref curencyRowSpanFlag, ref currency);

                    col.Index = index;
                    newDataList.Add(col);
                }
            }

            return newDataList;
        }

        private static void CalculateIndexRowSpan(ExtraPayout col, int numOfItem, ref bool rowSpanFlag, ref int index)
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
        }

        private static void CalculateProductRowSpan(IList<ExtraPayout> temps, ExtraPayout col, ref bool productRowSpanFlag, int currency, ref string product)
        {
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
        }

        private static void CalculateCurrencyRowSpan(IList<ExtraPayout> temps, ExtraPayout col, ref bool curencyRowSpanFlag, ref int currency)
        {
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
        }

        private static bool IsSportType(int betType)
        {
            return betType == BetType.VgamingFreeBets || betType == BetType.SCBFreeBets;
        }

        public string GetPromotionName(int promotionType, bool isHideSaba)
        {
            promotionType = _extraPayoutMapType.TryGetValue(promotionType, out int mapExtraPayoutType) ? mapExtraPayoutType : promotionType;

            if (SBPromotionBettypes.Contains(promotionType))
            {
                return $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(promotionType, isHideSaba)}";
            }

            if (CasinoPromotionBettypes.Contains(promotionType))
            {
                return $"{ViewResources.CasinoPromotion} - {_cacheRefAdapter.GetBettype(promotionType, isHideSaba)}";
            }

            if (promotionType == BetType.OddsBoostAdjustment)
            {
                return $"{ViewResources.SB} - {ViewResources.OddsBoostBonus}";
            }

            return IsSportType(promotionType) ? _cacheRefAdapter.GetSportName(promotionType) : _cacheRefAdapter.GetBettype(promotionType, isHideSaba);
        }
    }
}