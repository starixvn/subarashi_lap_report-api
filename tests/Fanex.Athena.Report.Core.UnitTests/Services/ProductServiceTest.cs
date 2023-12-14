using System.Linq;
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
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class ProductServiceTest
    {
        private const string cacheKey = "FunctionAllowedProducts";

        private readonly Mock<AppSettings> _appSetting;

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
            _appSetting = new Mock<AppSettings>();
            _productRepository = new Mock<IProductRepository>();
        }

        [Fact]
        public void GetProductList_CacheNotContainsData_ReturnResultFromDataAccess()
        {
            // Arrange
            var expectedResult = FakeHelper.CreateMany<ProductInfo>(10).ToList();
            _productRepository.Setup(x => x.GetProductAccessList(_licenseeId)).Returns(expectedResult);

            var service = new ProductService(_memoryCache, _productRepository.Object, _appSetting.Object,
                _webHostEnvironment.Object);

            // Action
            var result = service.GetProductList(_licenseeId, _languageCode);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void GetProductList_GetDataFromCache_ReturnCachedData()
        {
            // Arrange
            var expectedResult = FakeHelper.CreateMany<ProductInfo>(10).ToList();

            _productRepository.Setup(x => x.GetProductAccessList(_licenseeId)).Returns(expectedResult);
            _memoryCache.Set(cacheKey, expectedResult);

            var service = new ProductService(_memoryCache, _productRepository.Object, _appSetting.Object,
                _webHostEnvironment.Object);

            // Action
            var result = service.GetProductList(_licenseeId, _languageCode);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
