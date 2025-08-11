namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BySite.Controllers;
    using BySite.DTOs;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.DTO;
    using Common.Services.Interfaces;
    using Common.Settings;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class LiveReportControllerTest
    {
        private const string ProductCodeAll = "ALL";

        private const int CurrencyId = 999;

        private readonly int[] _liveReportNoDetailLinkSports = { 1, 2, 3 };

        private static readonly DateTime BalanceUpDate = DateTimeHelper.CreateDateTime(2017, 3, 4);

        private readonly AppSettings _appSettings;

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<IBaseCurrencyService> _baseCurrencyService;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ILiveReportService> _liveReportService;

        private readonly Mock<IProductService> _productService;

        public LiveReportControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _liveReportService = new Mock<ILiveReportService>();
            _productService = new Mock<IProductService>();
            _balanceUpService = new Mock<IBalanceUpService>();
            _baseCurrencyService = new Mock<IBaseCurrencyService>();
            _appSettings = FakeHelper.Create<AppSettings>();

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(new BalanceUp
            {
                BalanceUpDay = BalanceUpDate
            });

            _contextService.Setup(x => x.LicenseeId).Returns(1);
            _contextService.Setup(x => x.Language).Returns("Any");
        }

        [Fact]
        public async Task Summary_WhenCall_CallCorrectBehaviors()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportMetaModel>();
            const string ProductCodeList = "SB,BA";

            var productAccess = new ProductAccess(new List<ProductInfo>
            {
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "SB"),
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "BA")
            });

            _productService.Setup(x => x.GetProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.LiveReport)).Returns(productAccess);

            var controller = CreateController();

            // Action
            await controller.Summary(metaModel);

            // Assert
            _contextService.Verify(x => x.OfferedSubProductList, Times.Once);
            _productService.Verify(
                x => x.GetAllowedSubProductCodeList(It.IsAny<IEnumerable<string>>(), ProductCodeList), Times.Once);
            _productService.Verify(
                x => x.GetProductAccess(_contextService.Object.LicenseeId,
                    FunctionNames.LiveReport), Times.Once);
            _liveReportService.Verify(x => x.GetLiveReportSummary(metaModel), Times.Once);
        }

        [Fact]
        public async Task Detail_ProductCodeAll_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportMetaModel>();
            metaModel.FromDate = DateTime.Now;
            metaModel.ProductCode = ProductCodeAll;
            const string ProductCodeList = "SB,BA";

            var productAccess = new ProductAccess(new List<ProductInfo>
            {
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "SB"),
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "BA")
            });

            _productService.Setup(x => x.GetProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.LiveReport)).Returns(productAccess);
            _productService.Setup(x => x.GetProductSetting())
                .ReturnsAsync(new ProductSetting { LiveReportNoDetailLinkSports = _liveReportNoDetailLinkSports });

            var fakeResut = FakeHelper.Create<LiveReportDetailResultModel>();
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel, It.IsAny<IDictionary<int, string>>(),
                It.IsAny<IEnumerable<ProductInfo>>(), _liveReportNoDetailLinkSports)).ReturnsAsync(fakeResut);

            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);
            _contextService.Setup(x => x.CurrencyList).Returns(FakeHelper.Create<IReadOnlyDictionary<int, string>>());

            var controller = CreateController();

            // Action
            var response = await controller.Detail(metaModel) as OkObjectResult;
            var result = response.Value as LiveReportDetailResultModel;

            // Assert
            _contextService.Verify(x => x.OfferedSubProductList, Times.Once);
            _productService.Verify(
                x => x.GetAllowedSubProductCodeList(It.IsAny<IEnumerable<string>>(), metaModel.OriginalProductCodeList),
                Times.Once);
            _productService.Verify(
                x => x.GetProductAccess(_contextService.Object.LicenseeId,
                    FunctionNames.LiveReport), Times.Once);
            Assert.Equal(ProductCodeList, metaModel.OriginalProductCodeList);
            Assert.True(result.IsAllowedCustomerCommission);
            Assert.True(result.IsShowedDetailLink);
        }

        [Fact]
        public async Task Detail_SpecificProductCode_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportMetaModel>();
            metaModel.FromDate = DateTime.Now;
            metaModel.ProductCode = "BA";

            var productAccess = new ProductAccess(new List<ProductInfo>
            {
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "SB"),
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "BA")
            });

            _productService.Setup(x => x.GetProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.LiveReport)).Returns(productAccess);
            _productService.Setup(x => x.GetProductSetting())
                .ReturnsAsync(new ProductSetting { LiveReportNoDetailLinkSports = _liveReportNoDetailLinkSports });

            var fakeResut = FakeHelper.Create<LiveReportDetailResultModel>();
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel, It.IsAny<IDictionary<int, string>>(),
                It.IsAny<IEnumerable<ProductInfo>>(), _liveReportNoDetailLinkSports)).ReturnsAsync(fakeResut);

            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);
            _contextService.Setup(x => x.CurrencyList).Returns(FakeHelper.Create<IReadOnlyDictionary<int, string>>());

            var controller = CreateController();

            // Action
            var response = await controller.Detail(metaModel) as OkObjectResult;
            var result = response.Value as LiveReportDetailResultModel;

            // Assert
            _contextService.Verify(x => x.OfferedSubProductList, Times.Once);
            _productService.Verify(
                x => x.GetAllowedSubProductCodeList(It.IsAny<IEnumerable<string>>(), metaModel.OriginalProductCodeList),
                Times.Once);
            _productService.Verify(
                x => x.GetProductAccess(_contextService.Object.LicenseeId,
                    FunctionNames.LiveReport), Times.Once);
            Assert.Equal("BA", metaModel.OriginalProductCodeList);
            Assert.True(result.IsAllowedCustomerCommission);
            Assert.True(result.IsShowedDetailLink);
        }

        [Fact]
        public async Task MatchList_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportMatchListMetalModel>();
            var fakeData = FakeHelper.Create<SortableList<LiveReportWinLossByMatch>>();

            _liveReportService.Setup(x => x.GetLiveReportWinLossByMatch(metaModel)).ReturnsAsync(fakeData);
            _appSettings.HasAmountDueToColumn = true;
            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);

            var productAccess = new ProductAccess(new List<ProductInfo>
            {
                FakeHelper.Create<ProductInfo>(x => x.ProductCode, "BA")
            });

            _productService.Setup(x => x.GetProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.LiveReport)).Returns(productAccess);

            var controller = CreateController();

            // Action
            var response = await controller.MatchList(metaModel) as OkObjectResult;
            var result = response.Value as LiveReportMatchListResultModel;

            // Assert
            Assert.Equal(fakeData, result.Data);
            Assert.True(result.HasAmountDueToColumn);
            Assert.True(result.IsAllowedCustomerCommission);
        }

        [Fact]
        public async Task MatchListSort_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportMatchListMetalModel>();

            var fakeResult = FakeHelper.Create<SortableList<LiveReportWinLossByMatch>>();
            _liveReportService.Setup(x => x.GetLiveReportWinLossByMatch(metaModel)).ReturnsAsync(fakeResult);

            var controller = CreateController();

            // Action
            var response = await controller.MatchListSort(metaModel) as OkObjectResult;
            var result = response.Value as SortableList<LiveReportWinLossByMatch>;

            // Assert
            Assert.All(result, item => fakeResult.Contains(item));
            Assert.Equal(result.Count, fakeResult.Count);
        }

        private LiveReportController CreateController()
        {
            var controller = new LiveReportController(
                _liveReportService.Object,
                _balanceUpService.Object,
                _contextService.Object,
                _appSettings,
                _productService.Object,
                _baseCurrencyService.Object);

            return controller;
        }
    }
}