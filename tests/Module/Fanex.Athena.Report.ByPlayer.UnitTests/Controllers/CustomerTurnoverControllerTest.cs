using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Models;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerTurnoverControllerTest
    {
        private readonly AppSettings _appSettings;

        private readonly CustomerTurnoverController _controller;

        private readonly Mock<IBetTypeService> _betTypeService;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ICustomerTurnoverService> _customerTurnoverService;

        private readonly IReadOnlyDictionary<int, string> _currencyList = FakeHelper.Create<IReadOnlyDictionary<int, string>>();

        public CustomerTurnoverControllerTest()
        {
            _betTypeService = new Mock<IBetTypeService>();
            _customerTurnoverService = new Mock<ICustomerTurnoverService>();

            _appSettings = new AppSettings();

            _contextService = new Mock<IContextService>();
            _contextService.Setup(x => x.Language).Returns("Any");
            _contextService.Setup(x => x.CurrencyList).Returns(_currencyList);

            _controller = CreateController();
        }

        [Fact]
        public async Task GetBettypesBySport_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var expected = FakeHelper.CreateMany<BettypeProductItem>().ToList();

            _contextService.Setup(x => x.IsHideSaba).Returns(true);

            var sportProducts = FakeHelper.CreateMany<SportProductItem>().ToList();
            _betTypeService.Setup(x => x.GetBettypes(sportProducts, _contextService.Object.Language, _contextService.Object.IsHideSaba)).ReturnsAsync(expected);

            // Action
            var response = await _controller.GetBettypesBySport(_betTypeService.Object, sportProducts) as OkObjectResult;
            var result = response.Value as List<BettypeProductItem>;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetBettypesBySport_EmptySportProducts_ReturnEmpty()
        {
            // Action
            var response = await _controller.GetBettypesBySport(_betTypeService.Object, null) as OkObjectResult;
            var result = response.Value as List<BettypeProductItem>;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetData_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerTurnoverMetaModel>();
            var expected = FakeHelper.Create<ReportResultModel<CustTurnover>>();
            _customerTurnoverService.Setup(x => x.GetDataList(metaModel, _contextService.Object.CurrencyList, _contextService.Object.Language)).ReturnsAsync(expected);

            // Action
            var response = await _controller.GetData(_customerTurnoverService.Object, metaModel) as OkObjectResult;
            var result = response.Value as CustomerTurnoverResultModel;

            // Assert
            Assert.Equal(expected.DataList, result.Data);
            Assert.Equal(expected.TotalData, result.TotalData);
            Assert.False(metaModel.IsExportExcel);
        }

        private CustomerTurnoverController CreateController()
        {
            return new CustomerTurnoverController(_appSettings, _contextService.Object);
        }
    }
}
