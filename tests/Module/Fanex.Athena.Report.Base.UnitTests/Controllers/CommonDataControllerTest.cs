using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Base.Controllers;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Base.UnitTests.Controllers
{
    public class CommonDataControllerTest
    {
        private const string DefaultLicenseeName = "12bet";

        private readonly Mock<IBetTypeService> _betTypeService;

        private readonly Mock<IMemoryCache> _cache;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<IProductService> _productService;

        private readonly Mock<ISportService> _sportService;

        public CommonDataControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _productService = new Mock<IProductService>();
            _betTypeService = new Mock<IBetTypeService>();
            _sportService = new Mock<ISportService>();
            _cache = new Mock<IMemoryCache>();

            _contextService.Setup(x => x.SiteName).Returns(DefaultLicenseeName);
            _contextService.Setup(x => x.UserId).Returns(FakeHelper.Create<int>());
            _contextService.Setup(x => x.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(x => x.AgentList).Returns(new Dictionary<int, int[]>());
        }

        [Fact]
        public async Task GetSports_WhenCall_ReturnCorrectData()
        {
            using var controller = CreateController();

            // arrange
            var productCode = FakeHelper.Create<string>();
            var parent = new SportProductItem
                { ParentId = -1, Id = FakeHelper.Create<int>(), Name = FakeHelper.Create<string>(), };
            var child = new SportProductItem
                { ParentId = parent.Id, Id = FakeHelper.Create<int>(), Name = FakeHelper.Create<string>(), };

            _sportService.Setup(x => x.GetSportsAndProducts(
                _contextService.Object.LicenseeId,
                _contextService.Object.Language,
                productCode)).ReturnsAsync(new List<SportProductItem> { parent, child, });

            _productService.Setup(x => x.GetAllowedSubProductCodeList(
                                _contextService.Object.OfferedSubProductList,
                                productCode))
                           .Returns(productCode);

            // action
            var result = (await controller.GetSports(productCode).ConfigureAwait(false)).ElementAt(0);

            // assert
            Assert.Equal(child.Id, result.GetType().GetProperty("value")!.GetValue(result));
            Assert.Equal(child.Name, result.GetType().GetProperty("label")!.GetValue(result));
            Assert.True((bool)result.GetType().GetProperty("status")!.GetValue(result)!);
        }

        [Fact]
        public async Task GetBetTypes_WhenCall_ReturnCorrectData()
        {
            using var controller = CreateController();

            // arrange
            var productCode = FakeHelper.Create<string>();
            var item = new CheckedItem { ID = FakeHelper.Create<int>(), Name = FakeHelper.Create<string>(), };

            _betTypeService.Setup(x => x.GetBetTypesByProduct(
                                _contextService.Object.LicenseeId,
                                productCode,
                                _contextService.Object.Language, false))
                           .ReturnsAsync(new List<CheckedItem> { item, });

            _productService.Setup(x => x.GetAllowedSubProductCodeList(
                                _contextService.Object.OfferedSubProductList,
                                productCode))
                           .Returns(productCode);

            // action
            var result = (await controller.GetBetTypes(productCode)).ElementAt(0);

            // assert
            Assert.Equal(item.ID, result.GetType().GetProperty("value").GetValue(result));
            Assert.Equal(item.Name, result.GetType().GetProperty("label")!.GetValue(result));
            Assert.True((bool)result.GetType().GetProperty("status")!.GetValue(result)!);
        }

        private CommonDataController CreateController() =>
            new(
                _contextService.Object,
                _productService.Object,
                _betTypeService.Object,
                _sportService.Object,
                _cache.Object);
    }
}
