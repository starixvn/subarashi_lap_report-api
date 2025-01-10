using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class SettledMatchCountControllerTest
    {
        private readonly Mock<AppSettings> _appSettings;

        private readonly Mock<IContextService> _contextService;

        private readonly Fixture _fixture = new();

        private readonly Mock<IProductService> _productService;

        private readonly Mock<ISettledMatchCountService> _service;

        public SettledMatchCountControllerTest()
        {
            _service = new Mock<ISettledMatchCountService>();
            _contextService = new Mock<IContextService>();
            _productService = new Mock<IProductService>();
            _appSettings = new Mock<AppSettings>();
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnResult()
        {
            // Arrange
            const int licenseeId = 1;
            _contextService.Setup(x => x.LicenseeId).Returns(licenseeId);
            const string language = "en";
            _contextService.Setup(x => x.Language).Returns(language);

            IList<SettledMatchCountResultModel> expected = _fixture.CreateMany<SettledMatchCountResultModel>().ToList();
            _service.Setup(x => x.GetDataList(It.IsAny<SettledMatchCountMetaModel>()))
                    .Returns(Task.FromResult(expected));

            using var controller = new SettledMatchCountController(_service.Object, _contextService.Object,
                _productService.Object, _appSettings.Object);

            // Act
            var response = await controller.Index() as OkObjectResult;
            var result = response.Value as IList<SettledMatchCountResultModel>;

            // Assert
            _productService.Verify(x => x.GetProductAccess(licenseeId, It.IsAny<string>()));
            Assert.Equal(expected, result);
        }
    }
}
