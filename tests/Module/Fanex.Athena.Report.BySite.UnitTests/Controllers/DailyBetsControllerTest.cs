using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
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