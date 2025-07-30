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
    using Fanex.Athena.Report.Core.Services.Interfaces;

    public class ExtraPayoutService : IExtraPayoutService
    {
        private readonly IExtraPayoutRepository _repository;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IProductService _productService;
        private readonly Dictionary<string, List<CheckedItem>> ProductToPromotionTypeMap;

        public ExtraPayoutService(IExtraPayoutRepository repository, ICacheRefAdapter cacheRefAdapter, IProductService productService)
        {
            _repository = repository;
            _cacheRefAdapter = cacheRefAdapter;
            _productService = productService;
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
                { Product.SG, AddPromotionTypesForProductSg() },
                { Product.HOT, AddPromotionTypesForProductHot() },
                { Product.MGLC, AddPromotionTypesForProductMglc() },
                { Product.RGRNG, AddPromotionTypesForProductRgrng() },
                { Product.PTRNG, AddPromotionTypesForProductPtrng() }
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
                    Name = $"{_productService.GetProductName(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPTournamentPrize)}", Checked = true },
                new ()
                { ID = ExtraPayoutMapType.PayoutPPPromotionPrize,
                    Name = $"{_productService.GetProductName(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPPromotionPrize)}", Checked = true },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutPPJackpotPrize,
                    Name = $"{_productService.GetProductName(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPJackpotPrize)}",
                    Checked = true
                },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutPPJackpotContribution,
                    Name = $"{_productService.GetProductName(Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPJackpotContribution)}",
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
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.NBABalanceAdjustment)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutFIFACWCScoreChallenge,
                    Name = $"{ViewResources.SBPromotion} - {_cacheRefAdapter.GetBettype(BetType.FIFACWCScoreChallenge)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSbc()
        {
            return new List<CheckedItem>
            {
                new()
                { ID = ExtraPayoutMapType.PayoutSCBFreeBets,
                    Name = $"{_productService.GetProductName(Product.SBC)} - {ViewResources.FreeBets}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSCBFreeBetsJackpotPrize,
                    Name = $"{_productService.GetProductName(Product.SBC)} - {_cacheRefAdapter.GetBettype(BetType.SCBFreeBetsJackpotPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutSCBFreeBetsJackpotContribution,
                    Name = $"{_productService.GetProductName(Product.SBC)} - {_cacheRefAdapter.GetBettype(BetType.SCBFreeBetsJackpotContribution)}",
                    Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductVga()
        {
            return new List<CheckedItem>
            {
                new()
                { ID = ExtraPayoutMapType.PayoutVgamingFreeBets,
                    Name = $"{_productService.GetProductName(Product.VGA)} - {ViewResources.FreeBets}", Checked = true },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutVgamingPromotionPrize,
                    Name = $"{_productService.GetProductName(Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingPromotionPrize)}", Checked = true
                },
                new()
                { ID = ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotPrize,
                    Name = $"{_productService.GetProductName(Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingFreeBetsJackpotPrize)}", Checked = true },
                new()
                { ID = ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotContribution,
                    Name = $"{_productService.GetProductName(Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingFreeBetsJackpotContribution)}",
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
                    Name = $"{_productService.GetProductName(Product.FAC)} - {_cacheRefAdapter.GetBettype(BetType.FACHAIPromotion)}", Checked = true }
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

        private List<CheckedItem> AddPromotionTypesForProductMglc()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutMicrogamingLivePromotion,
                    Name = $"{_productService.GetProductName(Product.MGLC)} - {_cacheRefAdapter.GetBettype(BetType.MicrogamingLivePromotion)}", Checked = true }
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
                    Name = $"{_productService.GetProductName(Product.MGRNG)} - {_cacheRefAdapter.GetBettype(BetType.MGRNGPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductL22()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.Live22Promotion,
                    Name = $"{_productService.GetProductName(Product.L22)} - {_cacheRefAdapter.GetBettype(BetType.Live22Promotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductFs()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutFastSpinPromotion,
                    Name = $"{_productService.GetProductName(Product.FS)} - {_cacheRefAdapter.GetBettype(BetType.FastSpinPromotionPrize)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductHot()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutHOTDOGPromotionPrize,
                    Name = $"{_productService.GetProductName(Product.HOT)} - {_cacheRefAdapter.GetBettype(BetType.HOTDOGPromotionPrize)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductSg()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutSGJackpotPrize,
                    Name = $"{_productService.GetProductName(Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGJackpotPrize)}", Checked = true },
                new() { ID = ExtraPayoutMapType.PayoutSGJackpotContribution,
                    Name = $"{_productService.GetProductName(Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGJackpotContribution)}", Checked = true },
                new() { ID = ExtraPayoutMapType.PayoutSGPromotion,
                    Name = $"{_productService.GetProductName(Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductAb()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutAllbetPromotion,
                    Name = $"{_productService.GetProductName(Product.AB)} - {_cacheRefAdapter.GetBettype(BetType.AllbetPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductRgrng()
        {
            return new List<CheckedItem>
            {
                new() { ID = ExtraPayoutMapType.PayoutRichGamingPromotion,
                    Name = $"{_productService.GetProductName(Product.RGRNG)} - {_cacheRefAdapter.GetBettype(BetType.RichGamingPromotion)}", Checked = true }
            };
        }

        private List<CheckedItem> AddPromotionTypesForProductPtrng()
        {
            return new List<CheckedItem>
            {
                new()
                {
                    ID = ExtraPayoutMapType.PayoutPTRNGJackpotPrize,
                    Name = $"{_productService.GetProductName(Product.PTRNG)} - {_cacheRefAdapter.GetBettype(BetType.PTRNGJackpotPrize)}",
                    Checked = true
                },
                new()
                {
                    ID = ExtraPayoutMapType.PayoutPTRNGJackpotContribution,
                    Name = $"{_productService.GetProductName(Product.PTRNG)} - {_cacheRefAdapter.GetBettype(BetType.PTRNGJackpotContribution)}",
                    Checked = true
                }
            };
        }
    }
}