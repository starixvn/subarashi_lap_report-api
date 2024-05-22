using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class LiveReportControllerTest
    {
        private const string ProductCodeAll = "ALL";

        private const int CurrencyId = 999;

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
                _contextService.Object.Language,
                FunctionNames.LiveReport)).Returns(productAccess);

            var controller = CreateController();

            // Action
            var result = await controller.Summary(metaModel);

            // Assert
            _contextService.Verify(x => x.OfferedSubProductList, Times.Once);
            _productService.Verify(
                x => x.GetAllowedSubProductCodeList(It.IsAny<IEnumerable<string>>(), ProductCodeList), Times.Once);
            _productService.Verify(
                x => x.GetProductAccess(_contextService.Object.LicenseeId, _contextService.Object.Language,
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
                _contextService.Object.Language,
                FunctionNames.LiveReport)).Returns(productAccess);

            var fakeResut = FakeHelper.Create<LiveReportDetailResultModel>();
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel, It.IsAny<IDictionary<int, string>>(),
                It.IsAny<IEnumerable<ProductInfo>>())).ReturnsAsync(fakeResut);

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
                x => x.GetProductAccess(_contextService.Object.LicenseeId, _contextService.Object.Language,
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
                _contextService.Object.Language,
                FunctionNames.LiveReport)).Returns(productAccess);

            var fakeResut = FakeHelper.Create<LiveReportDetailResultModel>();
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel, It.IsAny<IDictionary<int, string>>(),
                It.IsAny<IEnumerable<ProductInfo>>())).ReturnsAsync(fakeResut);

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
                x => x.GetProductAccess(_contextService.Object.LicenseeId, _contextService.Object.Language,
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
                _contextService.Object.Language,
                FunctionNames.LiveReport)).Returns(productAccess);

            var controller = CreateController();

            // Action
            var response = await controller.MatchList(metaModel) as OkObjectResult;
            var result = response.Value as LiveReportMatchListResultModel;

            // Assert
            Assert.Equal(fakeData, result.Data);
            Assert.True(result.IsAllowBa);
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
