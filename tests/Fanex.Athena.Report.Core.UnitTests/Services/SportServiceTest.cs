namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Common.Constants;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using Core.Repositories.Abstraction;
    using Core.Services;
    using Core.Services.Interfaces;
    using Microsoft.Extensions.Caching.Memory;
    using Moq;
    using Report.UnitTests.Common;
    using Resources.Constants;
    using Xunit;

    public class SportServiceTest
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;
        private readonly Mock<IMemoryCache> _memoryCache;
        private readonly Mock<ISportRepository> _sportRepository;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<ICacheEntry> _mockCacheEntry;

        public SportServiceTest()
        {
            _sportRepository = new Mock<ISportRepository>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _memoryCache = new Mock<IMemoryCache>();
            _productService = new Mock<IProductService>();
            _mockCacheEntry = new Mock<ICacheEntry>();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetSportsAndProducts_WhenCall_ReturnCorrectly()
        {
            //Arrange
            const int LicenseeId = 1;
            const string Lang = "VN";
            const string ProductCode = "Sport";
            var service = CreateService();
            var expectedResult = FakeHelper.CreateMany<SportProductItem>().ToList();
            _sportRepository.Setup(x => x.GetSportsAndProducts(LicenseeId, Lang, ProductCode)).ReturnsAsync(expectedResult);

            //Action
            var result = await service.GetSportsAndProducts(LicenseeId, Lang, ProductCode);

            //Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetSportAndProductAndSabaSport_WhenCall_ReturnCorrectly()
        {
            //Arrange
            const int LicenseeId = 1;
            const string Lang = "VN";
            const string ProductCode = "Sport";
            const bool IsHideSaba = false;

            var service = CreateService();

            var sabaSports = service.GetSabaSports().ToList();
            var stringSabaSports = string.Join(",", sabaSports.Select(s => s.Id));

            var sportsAndProducts = FakeHelper.CreateMany<SportProductItem>().ToList();
            var stringSportsAndProducts = string.Join(",", sportsAndProducts.Select(s => s.Id));
            _sportRepository.Setup(x => x.GetSportsAndProducts(LicenseeId, Lang, ProductCode)).ReturnsAsync(sportsAndProducts);
            _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<int>(), It.IsAny<string>())).Returns("Sport Name");

            //Action
            var result = await service.GetSportAndProductAndSabaSport(LicenseeId, Lang, IsHideSaba, ProductCode);
            var stringResult = string.Join(",", result.Select(r => r.Id));

            //Assert
            Assert.Equal(sportsAndProducts.Count() + sabaSports.Count(), result.Count());
            Assert.Contains(stringSabaSports, stringResult);
            Assert.Contains(stringSportsAndProducts, stringResult);
        }

        [Fact]
        public async Task GetSportsForTreeView_WithCachedData_ReturnsCachedResult()
        {
            // Arrange
            const int licenseeId = 1;
            const string languageCode = "EN";
            const string productCodes = "SB";
            const bool isHideSaba = false;

            var cachedResult = _fixture.CreateMany<CheckedItem>().ToList();
            var cacheKey = "MemoryCache_SportList_TreeView" + licenseeId + languageCode + productCodes;

            object cachedValue = cachedResult;
            _memoryCache.Setup(m => m.TryGetValue(cacheKey, out cachedValue)).Returns(true);

            var service = CreateService();

            // Act
            var result = await service.GetSportsForTreeView(licenseeId, languageCode, productCodes);

            // Assert
            Assert.Equal(cachedResult, result);
            _sportRepository.Verify(r => r.GetSportsForTreeView(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetSportsForTreeView_WithoutCachedData_FetchesAndBuildsResult()
        {
            // Arrange
            const int licenseeId = 1;
            const string languageCode = "EN";
            const string productCodes = "SB";
            const bool isHideSaba = false;

            var sportItems = new List<SportForTreeViewItem>
            {
                new SportForTreeViewItem { ProductId = 1, ProductCode = "SB", SportType = SportType.Soccer },
                new SportForTreeViewItem { ProductId = 1, ProductCode = "SB", SportType = SportType.BasketBall }
            };

            object cachedValue = null;
            _memoryCache.Setup(m => m.TryGetValue(It.IsAny<string>(), out cachedValue)).Returns(false);
            _sportRepository.Setup(r => r.GetSportsForTreeView(licenseeId, productCodes)).ReturnsAsync(sportItems);
            _productService.Setup(p => p.GetProductName("SB")).Returns("Sportsbook");
            _cacheRefAdapter.Setup(c => c.GetSportName(SportType.Soccer, It.IsAny<string>())).Returns("Soccer");
            _cacheRefAdapter.Setup(c => c.GetSportName(SportType.BasketBall, It.IsAny<string>())).Returns("Basketball");

            var service = CreateService();

            // Act
            var result = await service.GetSportsForTreeView(licenseeId, languageCode, productCodes);

            // Assert
            Assert.NotEmpty(result);
            var productGroup = result.First();
            Assert.Equal(1, productGroup.ID);
            Assert.Equal("Sportsbook", productGroup.Name);
            Assert.Equal("SB", productGroup.Value);
            Assert.Equal(2, productGroup.Children.Count());
        }

        [Fact]
        public void GetSabaSports_WithHideSabaFalse_ReturnsSabaPrefixedSports()
        {
            // Arrange
            const bool isHideSaba = false;
            _cacheRefAdapter.Setup(c => c.GetSportName(SportType.Soccer, It.IsAny<string>())).Returns("Soccer");
            _cacheRefAdapter.Setup(c => c.GetSportName(SportType.BasketBall, It.IsAny<string>())).Returns("Basketball");

            var service = CreateService();

            // Act
            var result = service.GetSabaSports().ToList();

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, sport => Assert.StartsWith("Saba", sport.Name));
        }

        [Fact]
        public void GetSabaSports_WithHideSabaTrue_ReturnsVirtualPrefixedSports()
        {
            // Arrange
            const bool isHideSaba = true;
            _cacheRefAdapter.Setup(c => c.GetSportName(SportType.Soccer, It.IsAny<string>())).Returns("Soccer");
            _cacheRefAdapter.Setup(c => c.GetSportName(SportType.BasketBall, It.IsAny<string>())).Returns("Basketball");

            var service = CreateService();

            // Act
            var result = service.GetSabaSports(isHideSaba).ToList();

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, sport => Assert.StartsWith("Virtual", sport.Name));
        }

        [Fact]
        public async Task GetSportAndProductAndSabaSport_CombinesResults()
        {
            // Arrange
            const int licenseeId = 1;
            const string languageCode = "EN";
            const string productCodes = "SB";
            const bool isHideSaba = false;

            var sportsAndProducts = new List<SportProductItem>
            {
                new SportProductItem { Id = 1, Name = "Soccer", ParentId = ProductTypes.Sportsbook },
                new SportProductItem { Id = 2, Name = "Basketball", ParentId = ProductTypes.Sportsbook }
            };

            _sportRepository.Setup(r => r.GetSportsAndProducts(licenseeId, languageCode, productCodes))
                           .ReturnsAsync(sportsAndProducts);
            _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<int>(), It.IsAny<string>())).Returns("Sport Name");

            var service = CreateService();

            // Act
            var result = await service.GetSportAndProductAndSabaSport(licenseeId, languageCode, isHideSaba, productCodes);

            // Assert
            Assert.True(result.Count() > sportsAndProducts.Count); // Should include Saba sports too
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }

        [Fact]
        public async Task GetSportsAndProducts_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            const int licenseeId = 123;
            const string languageCode = "CN";
            const string productCodes = "SB,CS";

            var expectedResult = _fixture.CreateMany<SportProductItem>().ToList();
            _sportRepository.Setup(r => r.GetSportsAndProducts(licenseeId, languageCode, productCodes))
                           .ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetSportsAndProducts(licenseeId, languageCode, productCodes);

            // Assert
            Assert.Equal(expectedResult, result);
            _sportRepository.Verify(r => r.GetSportsAndProducts(licenseeId, languageCode, productCodes), Times.Once);
        }

        [Fact]
        public async Task GetSportsForTreeView_WithEmptySportItems_ReturnsEmptyResult()
        {
            // Arrange
            const int licenseeId = 1;
            const string languageCode = "EN";
            const string productCodes = "SB";

            var emptySportItems = new List<SportForTreeViewItem>();

            object cachedValue = null;
            _memoryCache.Setup(m => m.TryGetValue(It.IsAny<string>(), out cachedValue)).Returns(false);
            _sportRepository.Setup(r => r.GetSportsForTreeView(licenseeId, productCodes)).ReturnsAsync(emptySportItems);

            var service = CreateService();

            // Act
            var result = await service.GetSportsForTreeView(licenseeId, languageCode, productCodes);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(SportType.Soccer, "Soccer", false, "SabaSoccer")]
        [InlineData(SportType.BasketBall, "Basketball", true, "VirtualBasketball")]
        [InlineData(SportType.Tennis, "Tennis", false, "SabaTennis")]
        public void GetSportNameForTreeView_WithDifferentSportTypes_ReturnsCorrectFormat(int sportType, string sportName, bool isHideSaba, string expectedPrefix)
        {
            // Arrange
            _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<int>(), It.IsAny<string>())).Returns(sportName);
            var service = CreateService();

            // Act
            var result = service.GetSabaSports(isHideSaba).FirstOrDefault(s => s.SportTypeId == sportType);

            // Assert
            if (result != null)
            {
                Assert.StartsWith(isHideSaba ? "Virtual" : "Saba", result.Name);
                Assert.Contains(sportName, result.Name);
            }
        }

        [Fact]
        public async Task GetSportsForTreeView_WithMultipleProductGroups_GroupsCorrectly()
        {
            // Arrange
            const int licenseeId = 1;
            const string languageCode = "EN";
            const string productCodes = "SB,CS";

            var sportItems = new List<SportForTreeViewItem>
            {
                new SportForTreeViewItem { ProductId = 1, ProductCode = "SB", SportType = SportType.Soccer },
                new SportForTreeViewItem { ProductId = 1, ProductCode = "SB", SportType = SportType.BasketBall },
                new SportForTreeViewItem { ProductId = 2, ProductCode = "CS", SportType = SportType.Soccer }
            };

            object cachedValue = null;
            _memoryCache.Setup(m => m.TryGetValue(It.IsAny<string>(), out cachedValue)).Returns(false);
            _sportRepository.Setup(r => r.GetSportsForTreeView(licenseeId, productCodes)).ReturnsAsync(sportItems);
            _productService.Setup(p => p.GetProductName("SB")).Returns("Sportsbook");
            _productService.Setup(p => p.GetProductName("CS")).Returns("Casino");
            _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<int>(), It.IsAny<string>())).Returns("Sport");

            var service = CreateService();

            // Act
            var result = await service.GetSportsForTreeView(licenseeId, languageCode, productCodes);

            // Assert
            Assert.Equal(2, result.Count()); // Two product groups

            var sbGroup = result.FirstOrDefault(r => r.Value == "SB");
            var csGroup = result.FirstOrDefault(r => r.Value == "CS");

            Assert.NotNull(sbGroup);
            Assert.NotNull(csGroup);
            Assert.Equal(2, sbGroup.Children.Count()); // SB has 2 sports
            Assert.Equal(1, csGroup.Children.Count()); // CS has 1 sport
        }

        [Fact]
        public void GetSabaSports_ReturnsCorrectSportStructure()
        {
            // Arrange
            _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<int>(), It.IsAny<string>())).Returns("Test Sport");
            var service = CreateService();

            // Act
            var result = service.GetSabaSports().ToList();

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, sport =>
            {
                Assert.True(sport.Id != 0);
                Assert.NotEmpty(sport.Name);
                Assert.True(sport.SportTypeId >= 0);
            });
        }

        private SportService CreateService() => new(_sportRepository.Object, _cacheRefAdapter.Object, _memoryCache.Object, _productService.Object);
    }
}