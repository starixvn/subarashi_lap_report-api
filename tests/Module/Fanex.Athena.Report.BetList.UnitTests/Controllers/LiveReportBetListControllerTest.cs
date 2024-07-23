using Fanex.Athena.Report.BetList.Controllers;
using Fanex.Athena.Report.BetList.MetaModels;
using Fanex.Athena.Report.BetList.Models.BetList;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Xunit;
using Fanex.Athena.Report.BetList.Models;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.BetList.UnitTests.Controllers
{
    public class LiveReportBetListControllerTest
    {
        protected const string ExcelFileName = "LiveReportBetList";

        private const string LiveReportSpKey = "Lrf_BetList_LiveReport";

        private const string MatchSpKey = "Lrf_BetList_LiveReport_SelWinlossByMatch";

        private const string OutRightSpKey = "Lrf_BetList_LiveReport_SelWinlossOutright";

        private const string NumberGameSpKey = "Lrf_BetList_LiveReport_SelWinlossNumberGame";

        private const string MixParlaySpKey = "Lrf_BetList_LiveReport_SelWinlossParlay";

        private const string VirtualSportSpKey = "Lrf_BetList_LiveReport_SelWinlossVHRacing";

        private readonly BalanceUp _balanceUp;

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<IBetListApiClient> _betListApiClient;

        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ILicenseeService> _licenseeService;

        public LiveReportBetListControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _licenseeService = new Mock<ILicenseeService>();
            _betListApiClient = new Mock<IBetListApiClient>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();

            _balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balanceUp);

            _contextService.Setup(x => x.Language).Returns("Any-Language");
        }

        #region Match
        [Fact]
        public async Task MatchExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.MatchExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(MatchSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task Match_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<BetListDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<BetListDataResultModel>("/LiveReportBetList/GetData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Match(metaModel, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListDataResultModel>;

            // Assert
            AssertClientPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(metaModel.FromDate.ToShortDateString(), resultValue.Settings.FromDate);
            Assert.Equal(metaModel.ToDate.ToShortDateString(), resultValue.Settings.ToDate);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(MatchSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task Match_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            
            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Match(metaModel, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(MatchSpKey, metaModel.UspKey);
        }
        #endregion

        #region Outright
        [Fact]
        public async Task OutRightExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.OutrightExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(OutRightSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task OutRight_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<BetListDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<BetListDataResultModel>("/LiveReportBetList/GetData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Outright(metaModel, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListDataResultModel>;

            // Assert
            AssertClientPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(metaModel.FromDate.ToShortDateString(), resultValue.Settings.FromDate);
            Assert.Equal(metaModel.ToDate.ToShortDateString(), resultValue.Settings.ToDate);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(OutRightSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task OutRight_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Outright(metaModel, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(OutRightSpKey, metaModel.UspKey);
        }
        #endregion

        #region NumberGame
        [Fact]
        public async Task NumberGameExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.IsServerPaging = false;

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.NumberGameExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(NumberGameSpKey, metaModel.UspKey);
            Assert.True(metaModel.IsServerPaging);
        }

        [Fact]
        public async Task NumberGame_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<LiveReportBetListPagingDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<LiveReportBetListPagingDataResultModel>("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.NumberGame(metaModel, 1, 2, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<LiveReportBetListPagingDataResultModel>;

            // Assert
            AssertServerPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(metaModel.FromDate.ToShortDateString(), resultValue.Settings.FromDate);
            Assert.Equal(metaModel.ToDate.ToShortDateString(), resultValue.Settings.ToDate);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(NumberGameSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task NumberGame_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.NumberGame(metaModel, 1, 2, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.True(metaModel.IsServerPaging);
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(NumberGameSpKey, metaModel.UspKey);
        }
        #endregion

        #region MixParlay
        [Fact]
        public async Task MixParlayExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.IsServerPaging = false;

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.MixParlayExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(MixParlaySpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task MixParlay_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<LiveReportBetListPagingDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<LiveReportBetListPagingDataResultModel>("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.MixParlay(metaModel, 1, 2, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<LiveReportBetListPagingDataResultModel>;

            // Assert
            AssertServerPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(metaModel.FromDate.ToShortDateString(), resultValue.Settings.FromDate);
            Assert.Equal(metaModel.ToDate.ToShortDateString(), resultValue.Settings.ToDate);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(MixParlaySpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task MixParlay_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.MixParlay(metaModel, 1, 2, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(MixParlaySpKey, metaModel.UspKey);
        }
        #endregion

        #region VirtualSports
        [Fact]
        public async Task VirtualSportsExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.IsServerPaging = false;

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.VirtualSportsExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(VirtualSportSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task VirtualSports_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<LiveReportBetListPagingDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<LiveReportBetListPagingDataResultModel>("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.VirtualSports(metaModel, 1, 2, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<LiveReportBetListPagingDataResultModel>;

            // Assert
            AssertServerPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(VirtualSportSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task VirtualSports_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.VirtualSports(metaModel, 1, 2, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(VirtualSportSpKey, metaModel.UspKey);
        }
        #endregion

        #region SportsLottery
        [Fact]
        public async Task SportsLotteryExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.IsServerPaging = false;

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.SportsLotteryExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
            Assert.Contains(Product.SLT, metaModel.ProductCodeList);
        }

        [Fact]
        public async Task SportsLottery_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<LiveReportBetListPagingDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<LiveReportBetListPagingDataResultModel>("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.SportsLottery(metaModel, 1, 2, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<LiveReportBetListPagingDataResultModel>;

            // Assert
            AssertServerPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
            Assert.Contains(Product.SLT, metaModel.ProductCodeList);
        }

        [Fact]
        public async Task SportsLottery_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.SportsLottery(metaModel, 1, 2, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
            Assert.Contains(Product.SLT, metaModel.ProductCodeList);
        }
        #endregion

        #region BitCoin
        [Fact]
        public async Task BitCoinExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.IsServerPaging = false;

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.BitCoinExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
            Assert.Contains(Product.BTC, metaModel.ProductCodeList);
        }

        [Fact]
        public async Task BitCoin_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<LiveReportBetListPagingDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<LiveReportBetListPagingDataResultModel>("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.BitCoin(metaModel, 1, 2, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<LiveReportBetListPagingDataResultModel>;

            // Assert
            AssertServerPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(metaModel.FromDate.ToShortDateString(), resultValue.Settings.FromDate);
            Assert.Equal(metaModel.ToDate.ToShortDateString(), resultValue.Settings.ToDate);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
            Assert.Contains(Product.BTC, metaModel.ProductCodeList);
        }

        [Fact]
        public async Task BitCoin_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.BitCoin(metaModel, 1, 2, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
            Assert.Contains(Product.BTC, metaModel.ProductCodeList);
        }
        #endregion

        #region Vs3

        [Fact]
        public async Task Vs3ExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.IsServerPaging = false;

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.VS3ExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcelLiveReport", metaModel, _contextService.Object.Language), Times.Once);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task Vs3_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<LiveReportBetListPagingDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<LiveReportBetListPagingDataResultModel>("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.VS3(metaModel, 1, 2, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<LiveReportBetListPagingDataResultModel>;

            // Assert
            AssertServerPagingResultWithSetting(metaModel, resultValue);
            Assert.Equal(metaModel.FromDate.ToShortDateString(), resultValue.Settings.FromDate);
            Assert.Equal(metaModel.ToDate.ToShortDateString(), resultValue.Settings.ToDate);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
        }

        [Fact]
        public async Task Vs3_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<LiveReportBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient.Setup(x => x.PostStringAsync("/LiveReportBetList/GetPagingData", metaModel, _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.VS3(metaModel, 1, 2, true) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(LiveReportSpKey, metaModel.UspKey);
        }
        #endregion

        #region TestHelpers
        private void AssertServerPagingResultWithSetting<T>(BetListMetaModel metaModel, BetListResultModel<T> resultValue)
        {
            AssertResultWithSetting(metaModel, resultValue);
            Assert.True(resultValue.Settings.IsServerPaging);
        }

        private void AssertClientPagingResultWithSetting<T>(BetListMetaModel metaModel, BetListResultModel<T> resultValue)
        {
            AssertResultWithSetting(metaModel, resultValue);
            Assert.False(resultValue.Settings.IsServerPaging);
        }

        private void AssertResultWithSetting<T>(BetListMetaModel metaModel, BetListResultModel<T> resultValue)
        {
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.Title));
            Assert.Equal(metaModel.BaseCurrency, resultValue.Settings.BaseCurrency);
            Assert.Equal(_contextService.Object.IsAllowedCustomerComm, resultValue.Settings.IsAllowedCustomerCommission);
        }

        private LiveReportBetListController CreateControllerWithDefaultContext()
        {
            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var httpContext = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            httpContext.Setup(c => c.Request).Returns(request.Object);
            httpContext.Setup(c => c.Response).Returns(response.Object);

            var routeData = new RouteData();
            routeData.Values.Add("controller", "LiveReportBetList");

            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object,
                RouteData = routeData
            };

            return controller;
        }

        private LiveReportBetListController CreateController() => new(_contextService.Object, _licenseeService.Object,
            _betListApiClient.Object, _balanceUpService.Object, _cacheRefAdapter.Object);


        #endregion
    }
}
