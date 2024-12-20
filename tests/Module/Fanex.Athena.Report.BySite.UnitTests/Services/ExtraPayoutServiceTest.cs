using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class ExtraPayoutServiceTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IExtraPayoutRepository> _repository;

        public ExtraPayoutServiceTest()
        {
            _repository = new Mock<IExtraPayoutRepository>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMainData()
        {
            // Arrange
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>().With(_ => _.DrillDownLevel, 0).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayout>>>();

            _repository.Setup(_ => _.GetExtraPayoutMainData(metaModel)).Returns(expectedData);

            var service = CreateService();

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnMemberData()
        {
            // Arrange
            var metaModel = _fixture.Build<ExtraPayoutMetaModel>().With(_ => _.DrillDownLevel, 1).Create();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayout>>>();

            _repository.Setup(_ => _.GetExtraPayoutMemberData(metaModel)).Returns(expectedData);

            var service = CreateService();

            // Action
            var result = service.GetDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void GetDataAsync_WhenCall_ReturnDetailData()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
            var expectedData = _fixture.Create<Task<SortableReportResultModel<ExtraPayoutDetail>>>();

            _repository.Setup(_ => _.GetExtraPayoutDetailData(metaModel)).Returns(expectedData);

            var service = CreateService();

            // Action
            var result = service.GetDetailDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedData.Result, result.Result);
        }

        [Fact]
        public void BuildPromotionTypeList_EmptyProductCode_ReturnEmptyList()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(string.Empty);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentPP_ReturnListWithProductPP()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.PP).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutPPTournamentPrize == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutPPPromotionPrize == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutPPJackpotPrize == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutPPJackpotContribution == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentSPR_ReturnListWithProductSPR()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.SPR).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSabaLuckySpin == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSabaLoyaltyPointsExchange == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSabaRewardRiskFreeRebate == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSabaRedEnvelope == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSabaCasinoRoyaleTournamentPrize == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentSAG_ReturnListWithProductSAG()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.SAG).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSAGamingTip == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentSBC_ReturnListWithProductSBC()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.SBC).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSCBFreeBets == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSCBFreeBetsJackpotPrize == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSCBFreeBetsJackpotContribution == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentSXG_ReturnListWithProductSXG()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.SXG).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutEASexyLuckyDraw == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentCC_ReturnListWithProductCC()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.CC).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutArcadiaGamingBonus == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentVGA_ReturnListWithProductVGA()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.VGA).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutVgamingFreeBets == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotPrize == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutVgamingFreeBetsJackpotContribution == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentSG_ReturnListWithProductSG()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.SG).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSGJackpotPrize == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSGJackpotContribution == item.ID);
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutSGPromotion == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentAB_ReturnListWithProductAB()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.AB).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutAllbetPromotion == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentTIP_ReturnListWithProductTIP()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.TIP).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutStreamerTips == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentMGRNG_ReturnListWithProductMGRNG()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.MGRNG).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.PayoutMGRNGPromotion == item.ID);
        }

        [Fact]
        public void BuildPromotionTypeList_PresentL22_ReturnListWithProductL22()
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = service.BuildPromotionTypeList(Product.L22).ToList();

            // Assert
            Assert.Contains(result, item => ExtraPayoutMapType.Live22Promotion == item.ID);
        }

        private ExtraPayoutService CreateService() =>
            new ExtraPayoutService(_repository.Object, _cacheRefAdapter.Object);
    }
}
