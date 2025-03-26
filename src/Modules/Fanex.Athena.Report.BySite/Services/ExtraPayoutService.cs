namespace Fanex.Athena.Report.BySite.Services
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.DataDomain;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Models.DTOs;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ExtraPayoutService : IExtraPayoutService
    {
        private readonly IExtraPayoutRepository _repository;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly Dictionary<string, List<CheckedItem>> ProductToPromotionTypeMap;

        public ExtraPayoutService(IExtraPayoutRepository repository, ICacheRefAdapter cacheRefAdapter)
        {
            _repository = repository;
            _cacheRefAdapter = cacheRefAdapter;
            ProductToPromotionTypeMap = new Dictionary<string, List<CheckedItem>>
            {
                { Product.SB, AddPromotionTypesForProductSb() },
                { Product.AB, AddPromotionTypesForProductAb() },
                { Product.SAG, AddPromotionTypesForProductSag() },
                { Product.SXG, AddPromotionTypesForProductSxg() },
                { Product.CC, AddPromotionTypesForProductCc() },
                { Product.TIP, AddPromotionTypesForProductTip() },
                { Product.MGRNG, AddPromotionTypesForProductMgrng() },
                { Product.L22, AddPromotionTypesForProductL22() },
                { Product.FS, AddPromotionTypesForProductFs() },
                { Product.FAC, AddPromotionTypesForProductFac() },
                { Product.ASK, AddPromotionTypesForProductAsk() },
                { Product.PP, AddPromotionTypesForProductPp() },
                { Product.SPR, AddPromotionTypesForProductSpr() },
                { Product.SBC, AddPromotionTypesForProductSbc() },
                { Product.VGA, AddPromotionTypesForProductVga() },
                { Product.SG, AddPromotionTypesForProductSg() }
            };
        }

        public async Task<SortableReportResultModel<ExtraPayout>> GetDataAsync(ExtraPayoutMetaModel metaModel)
        {
            if (metaModel.DrillDownLevel == (int)ReportLevel.Main)
            {
                return await _repository.GetExtraPayoutMainData(metaModel).ConfigureAwait(false);
            }

            return await _repository.GetExtraPayoutMemberData(metaModel).ConfigureAwait(false);
        }

        public async Task<SortableReportResultModel<ExtraPayoutDetail>> GetDetailDataAsync(ExtraPayoutDetailMetaModel metaModel)
        {
            return await _repository.GetExtraPayoutDetailData(metaModel).ConfigureAwait(false);
        }

        public IEnumerable<CheckedItem> BuildPromotionTypeList(string productCodeList)
        {
            var promotionTypeList = new List<CheckedItem>();

            if (string.IsNullOrEmpty(productCodeList))
            {
                return promotionTypeList;
            }

            foreach (var productCode in productCodeList.Split(','))
            {
                if (ProductToPromotionTypeMap.TryGetValue(productCode, out List<CheckedItem> checkedItems))
                {
                    promotionTypeList.AddRange(checkedItems);
                }
            }

            return promotionTypeList;
        }

        private List<CheckedItem> AddPromotionTypesForProductPp()
        {
            return new List<CheckedItem>
            {
                new ()
                { ID = ExtraPayoutMapType.PayoutPPTournamentPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPTournamentPrize)}", Checked = true },
                new ()
                { ID = ExtraPayoutMapType.PayoutPPPromotionPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPPromotionPrize)}", Checked = true },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutPPJackpotPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPJackpotPrize)}",
                    Checked = true
                },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutPPJackpotContribution,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPJackpotContribution)}",
                    Checked = true
                }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSpr()
        {
            return new List<CheckedItem>
            {
                new()
                { ID = ExtraPayoutMapType.PayoutSabaLuckySpin,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaLuckySpin)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSabaLoyaltyPointsExchange,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaLoyaltyPointsExchange)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSabaRewardRiskFreeRebate,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaRewardRiskFreeRebate)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSabaRedEnvelope,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaRedEnvelope)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSabaCoinsCashExchange,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaCoinsCashExchange)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutPromotionJackpotPrize,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.PromotionJackpotPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutPromotionCashPrize,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.PromotionCashPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSabaCasinoRoyaleTournamentPrize,
                    Name = $"{ViewResources.CasinoPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaCasinoRoyaleTournamentPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutNumberGameLuckyDrawPromotion,
                    Name = $"{ViewResources.NGPromotion} - {_cacheRefAdapter.GetBettype(BetType.NumberGameLuckyDrawPromotion)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutNBAPlayoffsLotteryPromotion,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.NBAPlayoffsLotteryPromotion)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutNBABalanceAdjustment,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.NBABalanceAdjustment)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSbc()
        {
            return new List<CheckedItem>
            {
                new()
                { ID = ExtraPayoutMapType.PayoutSCBFreeBets,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.SBC)} - {ViewResources.FreeBets}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSCBFreeBetsJackpotPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.SBC)} - {_cacheRefAdapter.GetBettype(BetType.SCBFreeBetsJackpotPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSCBFreeBetsJackpotContribution,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.SBC)} - {_cacheRefAdapter.GetBettype(BetType.SCBFreeBetsJackpotContribution)}",
                    Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductVga()
        {
            return new List<CheckedItem>
            {
                new()
                { ID = ExtraPayoutMapType.PayoutVgamingFreeBets,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.VGA)} - {ViewResources.FreeBets}", Checked = true },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutVgamingPromotionPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingPromotionPrize)}", Checked = true
                },
                new()
                { ID = ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingFreeBetsJackpotPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotContribution,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingFreeBetsJackpotContribution)}",
                    Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSb()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutOddsBoostAdjustment,
                    Name = $"{ViewResources.ProductSB} - {ViewResources.OddsBoostBonus}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductFac()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutFACHAIPromotion,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.FAC)} - {_cacheRefAdapter.GetBettype(BetType.FACHAIPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductAsk()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutPromotionASKMESLOT,
                    Name = $"{ViewResources.ProductASK} - {_cacheRefAdapter.GetBettype(BetType.ASKMESLOTPromotionPrize)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSag()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutSAGamingTip,
                    Name = _cacheRefAdapter.GetBettype(BetType.SAGamingTip), Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSxg()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutEASexyLuckyDraw,
                    Name = _cacheRefAdapter.GetBettype(BetType.EASexyLuckyDraw), Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductCc()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutArcadiaGamingBonus,
                    Name = _cacheRefAdapter.GetBettype(BetType.ArcadiaGamingBonus), Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductTip()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutStreamerTips,
                    Name = _cacheRefAdapter.GetBettype(BetType.TIPStreamerTips), Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductMgrng()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutMGRNGPromotion,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.MGRNG)} - {_cacheRefAdapter.GetBettype(BetType.MGRNGPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductL22()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.Live22Promotion,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.L22)} - {_cacheRefAdapter.GetBettype(BetType.Live22Promotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductFs()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutFastSpinPromotion,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.FS)} - {_cacheRefAdapter.GetBettype(BetType.FastSpinPromotionPrize)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSg()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutSGJackpotPrize,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGJackpotPrize)}", Checked = true },
                new() { ID = ExtraPayoutMapType.PayoutSGJackpotContribution,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGJackpotContribution)}", Checked = true },
                new() { ID = ExtraPayoutMapType.PayoutSGPromotion,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductAb()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutAllbetPromotion,
                    Name = $"{ViewResources.ResourceManager.GetString(Product.AB)} - {_cacheRefAdapter.GetBettype(BetType.AllbetPromotion)}", Checked = true }
            };
        }
    }
}