using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class SearchCustomerControllerTest
    {
        private readonly Mock<ISearchCustomerService> _searchCustomerService;

        private readonly Mock<IContextService> _contextService;

        public SearchCustomerControllerTest()
        {
            _searchCustomerService = new Mock<ISearchCustomerService>();
            _contextService = new Mock<IContextService>();
        }

        [Fact]
        public async Task SearchCustomer_WhenCall_ReturnCorrectData()
        {
            // Arrange
            const string Query = "test";
            const int UserId = 0;
            var expectedCustomerNames = new[] { "test1", "test2" };

            _contextService.Setup(x => x.AgentList).Returns(new Dictionary<int, int[]>());
            _contextService.Setup(x => x.UserId).Returns(UserId);
            _searchCustomerService.Setup(x => x.SearchCustomers(UserId, Query, It.IsAny<string>(), false))
                                  .ReturnsAsync(expectedCustomerNames);

            var controller = CreateController();
            // Action
            var result = await controller.SearchCustomer(Query) as OkObjectResult;
            var resultData = result.Value;

            // Assert
            Assert.Equal(Query.Trim(), resultData.GetType().GetProperty("query")!.GetValue(resultData, null));
            Assert.Equal(expectedCustomerNames, resultData.GetType().GetProperty("suggestions")!.GetValue(resultData, null));
        }

        [Fact]
        public async Task SearchCustomer_EmptyQuery_ReturnNull()
        {
            // Arrange
            const string Query = "";

            _contextService.Setup(x => x.AgentList).Returns(new Dictionary<int, int[]>());

            var controller = CreateController();

            // Action
            var result = await controller.SearchCustomer(Query);

            // Assert
            Assert.Null(result);
        }

        private SearchCustomerController CreateController()
        {
            return new SearchCustomerController(_searchCustomerService.Object, _contextService.Object);
        }
    }
}
