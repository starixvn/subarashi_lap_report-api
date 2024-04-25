using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BetList.Controllers;
using Fanex.Athena.Report.BetList.MetaModels;
using Fanex.Athena.Report.BetList.MetaModels.MatchWinLoss;
using Fanex.Athena.Report.BetList.Models.BetList;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BetList.UnitTests.Controllers
{
    public class MatchWinLossBetListControllerTest
    {
        protected const string SP_BETLIST_MATCHWL = "Lrf_BetList_MatchWL";

        protected const string SP_BETLIST_MATCHWL_OLD = "Lrf_BetList_MatchWL_Old";

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<IBetListApiClient> _betListApiClient;

        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ILicenseeService> _licenseeService;

        public MatchWinLossBetListControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _licenseeService = new Mock<ILicenseeService>();
            _betListApiClient = new Mock<IBetListApiClient>();
            _balanceUpService = new Mock<IBalanceUpService>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();

            _contextService.Setup(x => x.Language).Returns("Any-Language");
        }

        [Fact]
        public async Task Outright_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Outright(metaModel, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListDataResultModel>;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.Title));
            Assert.Equal(metaModel.BaseCurrency, resultValue.Settings.BaseCurrency);
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelPath));
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelFileName));
            Assert.False(resultValue.Settings.IsServerPaging);
        }

        [Fact]
        public async Task Outright_BeforeBalanceUpdate_CallAPIWithSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);

            var controller = CreateController();

            // Action
            var result = await controller.Outright(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetList",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL_OLD)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task Outright_AfterBalanceUpdate_CallAPIWithSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(11);

            var controller = CreateController();

            // Action
            var result = await controller.Outright(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetList",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task MatchBetType_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.MatchBetType(metaModel, 1, 1, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListPagingDataResultModel>;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.Title));
            Assert.Equal(metaModel.BaseCurrency, resultValue.Settings.BaseCurrency);
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelPath));
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelFileName));
            Assert.True(resultValue.Settings.IsServerPaging);
        }

        [Fact]
        public async Task MatchBetType_BeforeBalanceUpdate_CallAPIWithOldSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);

            var controller = CreateController();

            // Action
            var result = await controller.MatchBetType(metaModel, 1, 1);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetListPaging",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL_OLD)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task MatchBetType_AfterBalanceUpdate_CallAPIWithSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(11);

            var controller = CreateController();

            // Action
            var result = await controller.MatchBetType(metaModel, 1, 1);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetListPaging",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task BetTypeWinLoss_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.BetTypeWinLoss(metaModel, 1, 1, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListPagingDataResultModel>;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.Title));
            Assert.Equal(metaModel.BaseCurrency, resultValue.Settings.BaseCurrency);
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelPath));
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelFileName));
            Assert.True(resultValue.Settings.IsServerPaging);
        }

        [Fact]
        public async Task BetTypeWinLoss_BeforeBalanceUpdate_CallAPIWithOldSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);

            var controller = CreateController();

            // Action
            var result = await controller.BetTypeWinLoss(metaModel, 1, 1);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetListPaging",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL_OLD)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task BetTypeWinLoss_AfterBalanceUpdate_CallAPIWithSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(11);

            var controller = CreateController();

            // Action
            var result = await controller.BetTypeWinLoss(metaModel, 1, 1);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetListPaging",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL)),
                _contextService.Object.Language));
        }

        //
        [Fact]
        public async Task MixParlay_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            const bool IsAllowedCustomerCom = true;
            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(IsAllowedCustomerCom);

            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            var metaModel = FakeHelper.Create<MixParlayMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.MixParlay(metaModel, 1, 1, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListPagingDataResultModel>;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.Title));
            Assert.Equal(metaModel.BaseCurrency, resultValue.Settings.BaseCurrency);
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelPath));
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.ExcelFileName));
            Assert.True(resultValue.Settings.IsServerPaging);
            Assert.Equal(IsAllowedCustomerCom, resultValue.Settings.IsAllowedCustomerCommision);
        }

        [Fact]
        public async Task MixParlay_BeforeBalanceUpdate_CallAPIWithOldSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<MixParlayMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(-1);

            var controller = CreateController();

            // Action
            var result = await controller.MixParlay(metaModel, 1, 1);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetListPaging",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL_OLD)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task MixParlay_AfterBalanceUpdate_CallAPIWithSp()
        {
            // Arrange
            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<MixParlayMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(11);

            var controller = CreateController();

            // Action
            var result = await controller.MixParlay(metaModel, 1, 1);

            // Assert
            _betListApiClient.Verify(x => x.PostStringAsync("/BetList/GetBetListPaging",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL)),
                _contextService.Object.Language));
        }

        [Fact]
        public async Task ExportExcel_WhenCall_CallExcelAPIWithSp()
        {
            // Arrange
            const bool IsAllowedCustomerCom = true;
            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(IsAllowedCustomerCom);

            var balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);

            _licenseeService.Setup(x =>
                                 x.HasPermissionOnData(It.IsAny<IReadOnlyDictionary<int, int[]>>(), It.IsAny<int>(),
                                     It.IsAny<long>()))
                            .ReturnsAsync(true);

            var metaModel = FakeHelper.Create<BetListExcelMetaModel>();
            metaModel.FromDate = balanceUp.BalanceUpDay.AddDays(1);

            var controller = CreateController();

            // Action
            var result = await controller.ExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(x => x.PostByteArrayAsync("/BetList/ExportExcel",
                It.Is<BetListMetaModel>(model => model.UspKey.Equals(SP_BETLIST_MATCHWL)),
                _contextService.Object.Language));

            Assert.Equal(IsAllowedCustomerCom, metaModel.ShowingOption.CustomerCommVisible);
        }

        private MatchWinLossBetListController CreateControllerWithDefaultContext()
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
            routeData.Values.Add("controller", "MatchWinLossBetList");

            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object,
                RouteData = routeData
            };

            return controller;
        }

        private MatchWinLossBetListController CreateController() => new(_contextService.Object, _licenseeService.Object,
            _betListApiClient.Object, _balanceUpService.Object, _cacheRefAdapter.Object);
    }
}
