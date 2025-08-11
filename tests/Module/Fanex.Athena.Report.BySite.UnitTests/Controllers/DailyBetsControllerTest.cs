namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.DTOs;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Model.DTO;
    using Common.Model.ViewModel;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    public class DailyBetsControllerTest
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<IContextService> _contextService;

        public DailyBetsControllerTest()
        {
            _contextService = new Mock<IContextService>();
        }

        [Fact]
        public async Task Index_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var expectedData = _fixture.Create<ResultModel<AuditTicket>>();
            var service = new Mock<IDailyBetsService>();
            var auditCurrencyList = new List<AuditCurrency>();

            service.Setup(s => s.GetDataAsync(metaModel, auditCurrencyList)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                expectedData.DataList,
                Total = expectedData.TotalData,
                expectedData.TotalRows
            });

            var controller = new DailyBetsController(_contextService.Object);

            // Action
            var result = JsonConvert.SerializeObject((await controller.Index(metaModel, service.Object) as OkObjectResult).Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}