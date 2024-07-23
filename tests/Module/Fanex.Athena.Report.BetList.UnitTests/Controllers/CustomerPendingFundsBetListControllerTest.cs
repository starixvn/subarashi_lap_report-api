using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BetList.Controllers;
using Fanex.Athena.Report.BetList.MetaModels.PendingFunds;
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
    public class CustomerPendingFundsBetListControllerTest
    {
        private const string SpRunningKey = "Shard_Lrf_BetList_SelRunningByDate_Mem";

        private readonly BalanceUp _balanceUp;

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<IBetListApiClient> _betListApiClient;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ILicenseeService> _licenseeService;

        public CustomerPendingFundsBetListControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _licenseeService = new Mock<ILicenseeService>();
            _betListApiClient = new Mock<IBetListApiClient>();

            _balanceUp = FakeHelper.Create<BalanceUp>();
            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balanceUp);

            _contextService.Setup(x => x.Language).Returns("Any-Language");
        }

        [Fact]
        public async Task ExportExcel_WhenCall_CallExcelApi()
        {
            // Arrange
            var metaModel = FakeHelper.Create<PendingFundsBetListExcelMetaModel>();

            var controller = CreateControllerWithDefaultContext();

            // Action
            await controller.ExportExcel(metaModel);

            // Assert
            _betListApiClient.Verify(
                x => x.PostByteArrayAsync("/BetList/ExportExcel", metaModel, _contextService.Object.Language),
                Times.Once);
            Assert.Equal(SpRunningKey, metaModel.UspKey);
        }

        [Fact]
        public async Task Index_WithSettings_ReturnDataWithSetting()
        {
            // Arrange
            var metaModel = FakeHelper.Create<PendingFundsBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();
            metaModel.IsServerPaging = false;

            var dataResult = FakeHelper.Create<BetListDataResultModel>();
            _betListApiClient.Setup(x => x.PostAsync<BetListDataResultModel>("/BetList/GetBetList", metaModel,
                                  _contextService.Object.Language))
                             .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Index(metaModel, false) as OkObjectResult;
            var resultValue = result!.Value as BetListResultModel<BetListDataResultModel>;

            // Assert
            Assert.False(metaModel.IsServerPaging);
            Assert.False(string.IsNullOrWhiteSpace(resultValue!.Settings.Title));
            Assert.Equal(metaModel.BaseCurrency, resultValue.Settings.BaseCurrency);
            Assert.Equal(dataResult, resultValue!.Data);
            Assert.Equal(SpRunningKey, metaModel.UspKey);
        }

        [Fact]
        public async Task Index_DataOnly_ReturnData()
        {
            // Arrange
            var metaModel = FakeHelper.Create<PendingFundsBetListMetaModel>();
            metaModel.FromDate = _balanceUp.BalanceUpDay.AddDays(-1);
            metaModel.BaseCurrency = FakeHelper.Create<string>();

            var dataResult = FakeHelper.Create<string>();
            _betListApiClient
               .Setup(x => x.PostStringAsync("/BetList/GetBetList", metaModel, _contextService.Object.Language))
               .ReturnsAsync(dataResult);

            var controller = CreateControllerWithDefaultContext();

            // Action
            var result = await controller.Index(metaModel) as OkObjectResult;
            var resultValue = result!.Value as string;

            // Assert
            Assert.Equal(dataResult, resultValue);
            Assert.Equal(SpRunningKey, metaModel.UspKey);
        }

        #region TestHelpers

        private CustomerPendingFundsBetListController CreateControllerWithDefaultContext()
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
            routeData.Values.Add("controller", "CustomerPendingFundsBetList");

            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object,
                RouteData = routeData
            };

            return controller;
        }

        private CustomerPendingFundsBetListController CreateController() => new(_contextService.Object,
            _licenseeService.Object,
            _betListApiClient.Object, _balanceUpService.Object);

        #endregion
    }
}
