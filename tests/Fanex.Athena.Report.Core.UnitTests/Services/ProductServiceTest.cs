using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class ProductServiceTest
    {
        private const string CacheKey = "MemoryCache_Common_Product";

        private readonly AppSettings _appSetting;

        private readonly string _languageCode = FakeHelper.Create<string>();

        private readonly int _licenseeId = FakeHelper.Create<int>();

        private readonly MemoryCache _memoryCache;

        private readonly Mock<IProductRepository> _productRepository;

        private readonly Mock<IWebHostEnvironment> _webHostEnvironment;

        public ProductServiceTest()
        {
            var cacheMock = new Mock<IOptions<MemoryCacheOptions>>();
            cacheMock.Setup(x => x.Value).Returns(new MemoryCacheOptions { Clock = new SystemClock(), });
            _memoryCache = new MemoryCache(cacheMock.Object);

            _webHostEnvironment = new Mock<IWebHostEnvironment>();
            _appSetting = new AppSettings();
            _productRepository = new Mock<IProductRepository>();
        }

        [Fact]
        public void GetProductList_CacheNotContainsData_ReturnResultFromDataAccess()
        {
            // Arrange
            var expectedResult = FakeHelper.CreateMany<ProductInfo>(10).ToList();
            _productRepository.Setup(x => x.GetProductAccessList(_licenseeId)).Returns(expectedResult);

            var service = GetService();

            // Action
            var result = service.GetProductList(_licenseeId);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetProductList_GetDataFromCache_ReturnCachedData()
        {
            // Arrange
            var expectedResult = FakeHelper.CreateMany<ProductInfo>(10).ToList();

            _productRepository.Setup(x => x.GetProductAccessList(_licenseeId)).Returns(expectedResult);
            _memoryCache.Set(CacheKey + _licenseeId, expectedResult);

            var service = GetService();

            // Action
            var result = service.GetProductList(_licenseeId);

            // Assert
            Assert.Equal(expectedResult, result);
            _productRepository.Verify(x => x.GetProductAccessList(_licenseeId), Times.Never);
        }

        private ProductService GetService() => new(_memoryCache, _productRepository.Object, _appSetting,
            _webHostEnvironment.Object);
    }
}
