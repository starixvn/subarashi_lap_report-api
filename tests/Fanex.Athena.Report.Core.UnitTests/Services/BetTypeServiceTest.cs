using AutoFixture;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class BetTypeServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IBetTypeRepository> _betTypeRepository;
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;
        private readonly Mock<IProductService> _productService;

        public BetTypeServiceTest()
        {
            _betTypeRepository = new Mock<IBetTypeRepository>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _productService = new Mock<IProductService>();
        }

        [Fact]
        public async Task GetBettypes_WhenSportsIsNull_ReturnsEmptyList()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetBettypes(null, "en", false);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBettypes_WhenSportsIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var service = CreateService();
            var emptySports = new List<SportProductItem>();

            // Act
            var result = await service.GetBettypes(emptySports, "en", false);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBettypes_WhenSportsHasItems_ReturnsBetTypes()
        {
            // Arrange
            var sports = new List<SportProductItem>
            {
                new SportProductItem { Id = 1, ParentId = -1 },
                new SportProductItem { Id = 2, ParentId = 1 }
            };
            var betTypes = new List<(string ProductPrefix, int BetTypeId)>
            {
                ("SB", 1),
                ("SB", 2)
            };

            _betTypeRepository.Setup(x => x.GetBettypes(It.IsAny<string>())).Returns(Task.FromResult(betTypes.ToList() as IList<(string ProductPrefix, int BetTypeId)>));
            _cacheRefAdapter.Setup(x => x.GetBetTypeName(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                           .Returns("Test BetType");

            var service = CreateService();

            // Act
            var result = await service.GetBettypes(sports, "en", false);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, item => Assert.True(item.Checked));
        }

        [Fact]
        public async Task GetBetTypesByProduct_CallsRepository()
        {
            // Arrange
            var licenseeId = _fixture.Create<int>();
            var productCode = _fixture.Create<string>();
            var languageCode = _fixture.Create<string>();
            var isHideSaba = _fixture.Create<bool>();
            var expectedResult = new List<CheckedItem>
            {
                new CheckedItem { Value = "1", Name = "Bet Type 1", Checked = true },
                new CheckedItem { Value = "2", Name = "Bet Type 2", Checked = false }
            };

            _betTypeRepository.Setup(x => x.GetBetTypesByProduct(licenseeId, productCode, languageCode, isHideSaba))
                             .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetBetTypesByProduct(licenseeId, productCode, languageCode, isHideSaba);

            // Assert
            Assert.Equal(expectedResult, result);
            _betTypeRepository.Verify(x => x.GetBetTypesByProduct(licenseeId, productCode, languageCode, isHideSaba), Times.Once);
        }

        [Fact]
        public async Task GetBetTypeForProducts_ReturnsGroupedBetTypes()
        {
            // Arrange
            var licenseeId = _fixture.Create<int>();
            var lang = _fixture.Create<string>();
            var productCodes = _fixture.Create<string>();
            var isHideSaba = _fixture.Create<bool>();

            var betTypesFromRepo = new List<(string ProductCode, int BetType)>
            {
                ("SB", 1),
                ("SB", 2),
                ("CA", 3)
            };

            _betTypeRepository.Setup(x => x.GetBetTypeForProductsAsync(licenseeId, productCodes))
                             .Returns(Task.FromResult(betTypesFromRepo.AsEnumerable()));
            _cacheRefAdapter.Setup(x => x.GetBetTypeName(It.IsAny<int>(), lang, isHideSaba))
                           .Returns("Test BetType");
            _productService.Setup(x => x.GetProductName(It.IsAny<string>())).Returns("Test Product");

            var service = CreateService();

            // Act
            var result = await service.GetBetTypeForProducts(licenseeId, lang, productCodes, isHideSaba);

            // Assert
            Assert.NotEmpty(result);
            _betTypeRepository.Verify(x => x.GetBetTypeForProductsAsync(licenseeId, productCodes), Times.Once);
        }

        private BetTypeService CreateService()
        {
            return new BetTypeService(_betTypeRepository.Object, _cacheRefAdapter.Object, _productService.Object);
        }
    }
}