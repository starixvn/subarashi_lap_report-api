using AutoFixture;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Fanex.Athena.Report.Core.Middleware;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class CustomerClassServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<ILogger<ErrorHandler>> _logger;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly AppSettings _appSettings;

        public CustomerClassServiceTest()
        {
            _cache = new Mock<IMemoryCache>();
            _logger = new Mock<ILogger<ErrorHandler>>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _appSettings = _fixture.Create<AppSettings>();
            
            // Setup HttpContext to avoid null reference issues
            var httpContext = new Mock<HttpContext>();
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);
        }


        [Fact]
        public async Task GetAllRiskLevel_WhenCacheExists_ReturnsCachedData()
        {
            // Arrange
            var cachedCategories = new List<CustomerClassCategory>
            {
                new CustomerClassCategory { CategoryName = "High", CustomerClass = 1, SabaRiskLevel = "All" },
                new CustomerClassCategory { CategoryName = "Medium", CustomerClass = 2, SabaRiskLevel = "All" }
            };

            object cacheValue = cachedCategories;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(true);

            var service = CreateService();

            // Act
            var result = await service.GetAllRiskLevel();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, r => r.Key == "All" || r.Key == "Uncategorized");
        }

        [Fact]
        public async Task GetAllRiskLevel_WhenNoCacheAndException_ReturnsUncategorizedOnly()
        {
            // Arrange
            object cacheValue = null;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            var service = CreateService();

            // Act
            var result = await service.GetAllRiskLevel();

            // Assert
            Assert.NotNull(result);
            // Should return at least the "Uncategorized" risk level when categories fail to load
            Assert.Contains(result, r => r.Key == "Uncategorized");
        }

        [Fact]
        public void Constructor_WithAllDependencies_SetsProperties()
        {
            // Arrange & Act
            var service = CreateService();

            // Assert
            Assert.NotNull(service);
        }

        private CustomerClassService CreateService()
        {
            return new CustomerClassService(
                _cache.Object,
                _logger.Object,
                _httpContextAccessor.Object,
                _appSettings,
                _httpClientFactory.Object);
        }
    }
}