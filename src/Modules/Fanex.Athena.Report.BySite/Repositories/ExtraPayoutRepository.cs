namespace Fanex.Athena.Report.BySite.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.DataDomain;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Core.Services.Interfaces;
    using Data;
    using DTOs;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Interfaces;
    using MetaModels;
    using Resources;
    using Resources.Constants;

    public class ExtraPayoutRepository : IExtraPayoutRepository
    {
        private readonly IBalanceUpService _balanceUpService;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IContextService _contextService;
        private readonly IProductService _productService;

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
            { ExtraPayoutMapType.PayoutMicrogamingLivePromotion, BetType.MicrogamingLivePromotion },
            { ExtraPayoutMapType.PayoutOddsBoostAdjustment, BetType.OddsBoostAdjustment },
            { ExtraPayoutMapType.PayoutPromotionASKMESLOT, BetType.ASKMESLOTPromotionPrize },
            { ExtraPayoutMapType.PayoutNumberGameLuckyDrawPromotion, BetType.NumberGameLuckyDrawPromotion },
            { ExtraPayoutMapType.PayoutNBAPlayoffsLotteryPromotion, BetType.NBAPlayoffsLotteryPromotion },
            { ExtraPayoutMapType.PayoutNBABalanceAdjustment, BetType.NBABalanceAdjustment },
            { ExtraPayoutMapType.PayoutHOTDOGPromotionPrize, BetType.HOTDOGPromotionPrize },
            { ExtraPayoutMapType.PayoutRichGamingPromotion, BetType.RichGamingPromotion },
            { ExtraPayoutMapType.PayoutFIFACWCScoreChallenge, BetType.FIFACWCScoreChallenge },
            { ExtraPayoutMapType.PayoutPTRNGJackpotContribution, BetType.PTRNGJackpotContribution },
            { ExtraPayoutMapType.PayoutPTRNGJackpotPrize, BetType.PTRNGJackpotPrize },
            { ExtraPayoutMapType.PayoutVSTournament, BetType.VSTournament },
            { ExtraPayoutMapType.PayoutPlaytechLiveJackpotContribution, BetType.PlaytechLiveJackpotContribution },
            { ExtraPayoutMapType.PayoutPlaytechLiveJackpotPrize, BetType.PlaytechLiveJackpotPrize }
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
            BetType.PromotionCashPrize,
            BetType.NBAPlayoffsLotteryPromotion,
            BetType.NBABalanceAdjustment,
            BetType.FIFACWCScoreChallenge
        };

        private static readonly int[] CasinoPromotionBettypes = new[] {
            BetType.SabaCasinoRoyaleTournamentPrize
        };

        private static readonly int[] NumberGamePromotionBettypes = new[] {
            BetType.NumberGameLuckyDrawPromotion
        };

        public ExtraPayoutRepository(
            IBalanceUpService balanceUpService, ICacheRefAdapter cacheRefAdapter, IContextService contextService, IProductService productService)
        {
            _balanceUpService = balanceUpService;
            _cacheRefAdapter = cacheRefAdapter;
            _contextService = contextService;
            _productService = productService;
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
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo);

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
            data.ProductName = _productService.GetProductName(productCode);
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

        private static void UpdateSpCommonParamsAsync(IObjectDb db, ExtraPayoutMetaModel metaModel, BalanceUp balanceUpInfo)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency.ToUpperInvariant());
            db.SetParameterValue("fromdate", metaModel.DrillDownLevel == (int)ReportLevel.Main ? metaModel.FromDate.Date : metaModel.DrillDownFromDate.Date);
            db.SetParameterValue("todate", metaModel.DrillDownLevel == (int)ReportLevel.Main ? metaModel.ToDate.Date : metaModel.DrillDownToDate.Date);
            db.SetParameterValue("productList", metaModel.ProductCodeList);
            db.SetParameterValue("PromotionTypeList", metaModel.PromotionTypeIdList);
            db.SetParameterValue("movebetsday", balanceUpInfo.MoveBetsDay);
            db.SetParameterValue("BalanceUpDay", balanceUpInfo.BalanceUpDay);
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
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo);

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
            data.Username = reader.GetVal<string>("username");
            data.PromotionType = promotionType;
            data.PromotionName = GetPromotionName(promotionType, isHideSaba);
            data.Nickname = GetNicknameValue(reader, promotionType);
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
                UpdateSpCommonParamsAsync(db, metaModel, balanceUpInfo);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetDetailData(reader, metaModel);
                }
            }
        }

        public async Task<SortableReportResultModel<OddsBoostBonusDetail>> GetOddsBoostBonusDetailData(
            ExtraPayoutMetaModel metaModel)
        {
            const string UspName = "Lrf_Rpt_ExtraPayout_OddsBoost_Detail";
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync().ConfigureAwait(false);

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("AgentList", metaModel.AgentListXml);
                db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency.ToUpperInvariant());
                db.SetParameterValue("fromdate", metaModel.DrillDownFromDate.Date);
                db.SetParameterValue("todate", metaModel.DrillDownToDate.Date);
                db.SetParameterValue("movebetsday", balanceUpInfo.MoveBetsDay);
                db.SetParameterValue("BalanceUpDay", balanceUpInfo.BalanceUpDay);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    return GetOddsBoostBonusDetailData(reader, metaModel);
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
                    ProductName = _productService.GetProductName(productCode),
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

        private SortableReportResultModel<OddsBoostBonusDetail> GetOddsBoostBonusDetailData(IDataReader reader, ExtraPayoutMetaModel metaModel)
        {
            var dataList = new SortableList<OddsBoostBonusDetail>();

            while (reader.Read())
            {
                var date = reader.GetVal<DateTime>("WinLostDate");
                var dateString = date.ToString("MM/dd/yyyy");
                var productCode = reader.GetVal<string>("ProductCode");

                var item = new OddsBoostBonusDetail
                {
                    Date = date,
                    DateString = dateString,
                    ExternalId = reader.GetVal<string>("ExternalId"),
                    SystemId = reader.GetVal<string>("SystemId"),
                    RefNo = reader.GetVal<string>("RefNo"),
                    ProductCode = productCode,
                    ProductName = _productService.GetProductName(productCode),
                    PromotionType = ViewResources.OddsBoostBonus,
                    BasedCustomerAmount = reader.GetVal<decimal>("BasedCustomerAmount")
                };

                item.CustomerAmount = metaModel.IsConvertAllToBaseCurrency ? item.BasedCustomerAmount : reader.GetVal<decimal>("CustomerAmount");

                dataList.Add(item);
            }

            return new SortableReportResultModel<OddsBoostBonusDetail>
            {
                DataList = dataList,
                TotalData = GetSumOddsBoostBonusDetailData(dataList)
            };
        }

        private static OddsBoostBonusDetail GetSumOddsBoostBonusDetailData(List<OddsBoostBonusDetail> dataList)
        {
            if (dataList == null || !dataList.Any())
            {
                return new OddsBoostBonusDetail();
            }

            return new OddsBoostBonusDetail
            {
                CustomerAmount = dataList.Sum(_ => _.BasedCustomerAmount)
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

            if (NumberGamePromotionBettypes.Contains(promotionType))
            {
                return $"{ViewResources.NGPromotion} - {_cacheRefAdapter.GetBettype(promotionType, isHideSaba)}";
            }

            if (promotionType == BetType.OddsBoostAdjustment)
            {
                return $"{ViewResources.SB} - {ViewResources.OddsBoostBonus}";
            }

            return IsSportType(promotionType) ? _cacheRefAdapter.GetSportName(promotionType) : _cacheRefAdapter.GetBettype(promotionType, isHideSaba);
        }
    }
}