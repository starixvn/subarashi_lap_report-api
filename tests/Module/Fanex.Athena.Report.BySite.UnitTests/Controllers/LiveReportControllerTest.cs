namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using BySite.Controllers;
    using BySite.Excels.Interfaces;
    using BySite.Excels.Options;
    using BySite.Services.Interfaces;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Model.DTO;
    using Common.Services.Interfaces;
    using Common.Settings;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using DTOs;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class LiveReportControllerTest
    {
        private const string ProductCodeAll = "ALL";

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

            MockProductAccess(productAccess);

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

            MockProductAccess(productAccess);

            var fakeResult = FakeHelper.Create<LiveReportDetailResultModel>();
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel, It.IsAny<IDictionary<int, string>>(),
                It.IsAny<IEnumerable<ProductInfo>>())).ReturnsAsync(fakeResult);

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

            MockProductAccess(productAccess);

            var fakeResult = FakeHelper.Create<LiveReportDetailResultModel>();
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel, It.IsAny<IDictionary<int, string>>(),
                It.IsAny<IEnumerable<ProductInfo>>())).ReturnsAsync(fakeResult);

            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);
            _contextService.Setup(x => x.CurrencyList).Returns(FakeHelper.Create<IReadOnlyDictionary<int, string>>());

            var controller = CreateController();

            // Action
            var response = await controller.Detail(metaModel) as OkObjectResult;
            var result = response!.Value as LiveReportDetailResultModel;

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

            MockProductAccess(productAccess);

            var controller = CreateController();

            // Action
            var response = await controller.MatchList(metaModel) as OkObjectResult;
            var result = response!.Value as LiveReportMatchListResultModel;

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
            var result = response!.Value as SortableList<LiveReportWinLossByMatch>;

            // Assert
            Assert.All(result, item => fakeResult.Contains(item));
            Assert.Equal(result.Count, fakeResult.Count);
        }

        [Theory]
        [AutoData]
        public async Task MatchListExport_WhenCall_ReturnCorectResult(
            string template,
            LiveReportMatchListMetalModel metaModel,
            SortableList<LiveReportWinLossByMatch> result,
            ProductAccess productAccess)
        {
            // Arrange
            var cacheRefAdapter = new Mock<ICacheRefAdapter>();
            var excelService = new Mock<ILiveReportMatchListExcel>();

            MockProductAccess(productAccess);

            _liveReportService.Setup(x => x.GetLiveReportWinLossByMatch(metaModel)).ReturnsAsync(result);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(x => x.PhysicalPath).Returns(template);

            var file = new Mock<IFileProvider>();
            file.Setup(x => x.GetFileInfo("/ExcelTemplate/LiveReportMatchList.xls")).Returns(fileInfo.Object);

            var controller = CreateController();

            // Action
            var response = await controller.MatchListExport(file.Object, excelService.Object, cacheRefAdapter.Object, metaModel) as FileContentResult;

            // Assert
            Assert.NotNull(response);
            fileInfo.VerifyGet(x => x.PhysicalPath, Times.Once);
            excelService.Verify(x => x.Export(template, result, metaModel, It.IsAny<LiveReportMatchListExcelOption>()));
        }

        [Theory]
        [AutoData]
        public async Task ExportExcel_WhenCall_ReturnCorectResult(
            string template,
            List<LiveReport> summaryReports,
            LiveReportDetailResultModel result,
            LiveReportMetaModel metaModel,
            ProductAccess productAccess)
        {
            // Arrange
            var excelService = new Mock<ILiveReportExcel>();

            MockProductAccess(productAccess);

            _contextService.Setup(x => x.CurrencyList).Returns(FakeHelper.Create<Dictionary<int, string>>());

            _liveReportService.Setup(x => x.GetLiveReportSummary(metaModel)).ReturnsAsync(summaryReports);
            _liveReportService.Setup(x => x.GetLiveReportSummaryDetail(metaModel,
                    It.IsAny<Dictionary<int, string>>(),
                    It.IsAny<IEnumerable<ProductInfo>>()))
                .ReturnsAsync(result);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(x => x.PhysicalPath).Returns(template);

            var file = new Mock<IFileProvider>();
            file.Setup(x => x.GetFileInfo("/ExcelTemplate/LiveReport.xls")).Returns(fileInfo.Object);

            var controller = CreateController();

            // Action
            var response = await controller.ExportExcel(file.Object, excelService.Object, metaModel) as FileContentResult;

            // Assert
            Assert.NotNull(response);
            fileInfo.VerifyGet(x => x.PhysicalPath, Times.Once);
            excelService.Verify(x => x.Export(template, summaryReports, result, metaModel, It.IsAny<LiveReportExcelOption>()));
        }

        #region TestHelpers

        private void MockProductAccess(ProductAccess productAccess)
        {
            _productService.Setup(x => x.GetProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.LiveReport)).Returns(productAccess);
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

        #endregion
    }
}