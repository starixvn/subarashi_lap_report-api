namespace Fanex.Athena.Report.BySite.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.DataDomain;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Extensions;
    using Fanex.Athena.Report.Core.Models.DTOs;
    using Fanex.Athena.Report.Resources;
    using Fanex.Athena.Report.Resources.Constants;

    public class ExtraPayoutService : IExtraPayoutService
    {
        private readonly IExtraPayoutRepository _repository;
        private readonly ICacheRefAdapter _cacheRefAdapter;

        public ExtraPayoutService(IExtraPayoutRepository repository, ICacheRefAdapter cacheRefAdapter)
        {
            _repository = repository;
            _cacheRefAdapter = cacheRefAdapter;
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

        public IEnumerable<CheckedItem> BuildPromotionTypeList(string productCode)
        {
            var promotionTypeList = new List<CheckedItem>();

            if (string.IsNullOrEmpty(productCode))
            {
                return promotionTypeList;
            }
            const string ProductResource = "Product";
            var productCodeList = productCode.Split(',');

            AddPromotionTypesForProductPp(promotionTypeList, productCodeList, ProductResource);
            AddPromotionTypesForProductSpr(promotionTypeList, productCodeList);

            if (productCodeList.IsPresent(_ => _ == Product.SAG))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new() { ID = ExtraPayoutMapType.PayoutSAGamingTip, Name = _cacheRefAdapter.GetBettype(BetType.SAGamingTip), Checked = true }
                });
            }

            AddPromotionTypesForProductSbc(promotionTypeList, productCodeList, ProductResource);

            if (productCodeList.IsPresent(_ => _ == Product.SXG))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new() { ID = ExtraPayoutMapType.PayoutEASexyLuckyDraw, Name = _cacheRefAdapter.GetBettype(BetType.EASexyLuckyDraw), Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.CC))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new() { ID = ExtraPayoutMapType.PayoutArcadiaGamingBonus, Name = _cacheRefAdapter.GetBettype(BetType.ArcadiaGamingBonus), Checked = true }
                });
            }

            AddPromotionTypesForProductVga(promotionTypeList, productCodeList, ProductResource);
            AddPromotionTypesForProductSg(promotionTypeList, productCodeList, ProductResource);

            if (productCodeList.IsPresent(_ => _ == Product.AB))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutAllbetPromotion,
                        Name = $"{ViewResources.ResourceManager.GetString(ProductResource + Product.AB)} - {_cacheRefAdapter.GetBettype(BetType.AllbetPromotion)}", Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.TIP))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutStreamerTips,
                        Name = _cacheRefAdapter.GetBettype(BetType.TIPStreamerTips), Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.MGRNG))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutMGRNGPromotion,
                        Name = $"{ViewResources.ResourceManager.GetString(ProductResource + Product.MGRNG)} - {_cacheRefAdapter.GetBettype(BetType.MGRNGPromotion)}", Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.L22))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.Live22Promotion,
                        Name = $"{ViewResources.ResourceManager.GetString(ProductResource + Product.L22)} - {_cacheRefAdapter.GetBettype(BetType.Live22Promotion)}", Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.FS))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutFastSpinPromotion,
                        Name = $"{ViewResources.ResourceManager.GetString(ProductResource + Product.FS)} - {_cacheRefAdapter.GetBettype(BetType.FastSpinPromotionPrize)}", Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.FAC))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new() { ID = ExtraPayoutMapType.PayoutFACHAIPromotion,
                        Name = $"{ViewResources.ResourceManager.GetString(ProductResource + Product.FAC)} - {_cacheRefAdapter.GetBettype(BetType.FACHAIPromotion)}", Checked = true }
                });
            }

            if (productCodeList.IsPresent(_ => _ == Product.SB))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new CheckedItem { ID = ExtraPayoutMapType.PayoutOddsBoostAdjustment,
                        Name = $"{ViewResources.ProductSB} - {ViewResources.OddsBoostBonus}", Checked = true }
                });
            }

            return promotionTypeList;
        }

        private void AddPromotionTypesForProductPp(List<CheckedItem> promotionTypeList, IEnumerable<string> productCodeList, string productResource)
        {
            if (productCodeList.IsPresent(_ => _ == Product.PP))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutPPTournamentPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPTournamentPrize)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutPPPromotionPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPPromotionPrize)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutPPJackpotPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPJackpotPrize)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutPPJackpotContribution,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.PP)} - {_cacheRefAdapter.GetBettype(BetType.PPJackpotContribution)}", Checked = true }
                });
            }
        }

        private void AddPromotionTypesForProductSpr(List<CheckedItem> promotionTypeList, IEnumerable<string> productCodeList)
        {
            if (productCodeList.IsPresent(_ => _ == Product.SPR))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
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
                        Name = $"{ViewResources.CasinoPromotion} - {_cacheRefAdapter.GetBettype(BetType.SabaCasinoRoyaleTournamentPrize)}", Checked = true }
                });
            }
        }

        private void AddPromotionTypesForProductSbc(List<CheckedItem> promotionTypeList, IEnumerable<string> productCodeList, string productResource)
        {
            if (productCodeList.IsPresent(_ => _ == Product.SBC))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutSCBFreeBets,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.SBC)} - {ViewResources.FreeBets}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutSCBFreeBetsJackpotPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.SBC)} - {_cacheRefAdapter.GetBettype(BetType.SCBFreeBetsJackpotPrize)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutSCBFreeBetsJackpotContribution,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.SBC)} - {_cacheRefAdapter.GetBettype(BetType.SCBFreeBetsJackpotContribution)}",
                        Checked = true }
                });
            }
        }

        private void AddPromotionTypesForProductVga(List<CheckedItem> promotionTypeList, IEnumerable<string> productCodeList, string productResource)
        {
            if (productCodeList.IsPresent(_ => _ == Product.VGA))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutVgamingFreeBets,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.VGA)} - {ViewResources.FreeBets}", Checked = true },
                    new() 
                    { 
                        ID = ExtraPayoutMapType.PayoutVgamingPromotionPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingPromotionPrize)}", Checked = true 
                    },
                    new()
                    { ID = ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingFreeBetsJackpotPrize)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotContribution,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.VGA)} - {_cacheRefAdapter.GetBettype(BetType.VgamingFreeBetsJackpotContribution)}",
                        Checked = true }
                });
            }
        }

        private void AddPromotionTypesForProductSg(List<CheckedItem> promotionTypeList, IEnumerable<string> productCodeList, string productResource)
        {
            if (productCodeList.IsPresent(_ => _ == Product.SG))
            {
                promotionTypeList.AddRange(new List<CheckedItem>
                {
                    new()
                    { ID = ExtraPayoutMapType.PayoutSGJackpotPrize,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGJackpotPrize)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutSGJackpotContribution,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGJackpotContribution)}", Checked = true },
                    new()
                    { ID = ExtraPayoutMapType.PayoutSGPromotion,
                        Name = $"{ViewResources.ResourceManager.GetString(productResource + Product.SG)} - {_cacheRefAdapter.GetBettype(BetType.SGPromotion)}", Checked = true }
                });
            }
        }
    }

}