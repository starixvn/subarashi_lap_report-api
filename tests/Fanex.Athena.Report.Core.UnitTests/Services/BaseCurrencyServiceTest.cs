using AutoFixture;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class BaseCurrencyServiceTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<ICacheEntry> _cacheEntry;
        private readonly AppSettings _appSettings;

        public BaseCurrencyServiceTest()
        {
            _contextService = new Mock<IContextService>();
            _cache = new Mock<IMemoryCache>();
            _cacheEntry = new Mock<ICacheEntry>();
            _cache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(_cacheEntry.Object);
            _appSettings = _fixture.Create<AppSettings>();
        }

        [Fact]
        public void GetBaseCurrencyList_WhenCacheExists_ReturnsCachedValue()
        {
            // Arrange
            var licenseName = _fixture.Create<string>();
            var specialPlatform = _fixture.Create<int>();
            var userName = _fixture.Create<string>();
            var expectedCurrency = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };

            _contextService.Setup(x => x.SiteName).Returns(licenseName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);

            object cacheValue = expectedCurrency;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(true);

            var service = CreateService();

            // Act
            var result = service.GetBaseCurrencyList();

            // Assert
            Assert.Equal(expectedCurrency, result);
        }

        [Fact]
        public void GetBaseCurrencyList_WhenNoCacheAndCryptoLicensee_ReturnsCryptoCurrency()
        {
            // Arrange
            var licenseName = "CRYPTO_LICENSEE";
            var specialPlatform = _fixture.Create<int>();
            var userName = _fixture.Create<string>();

            _appSettings.OfferCryptoCurrencyLicensees = new[] { licenseName }.ToList();
            _contextService.Setup(x => x.SiteName).Returns(licenseName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string>());

            object cacheValue = null;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            var service = CreateService();

            // Act
            var result = service.GetBaseCurrencyList();

            // Assert
            Assert.NotNull(result);
            _cache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void GetBaseCurrencyList_WhenEurSupportLicensee_ReturnsEurSupportCurrency()
        {
            // Arrange
            var licenseName = "EUR_SUPPORT_LICENSEE";
            var specialPlatform = _fixture.Create<int>();
            var userName = _fixture.Create<string>();

            _appSettings.OfferCryptoCurrencyLicensees = new List<string>();
            _appSettings.CryptoLicenseesSupportBaseEur = new[] { licenseName }.ToList();
            _contextService.Setup(x => x.SiteName).Returns(licenseName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string>());

            object cacheValue = null;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            var service = CreateService();

            // Act
            var result = service.GetBaseCurrencyList();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBaseCurrencyList_WhenJollySportsPlatform_ReturnsJollySportsCurrency()
        {
            // Arrange
            var licenseName = _fixture.Create<string>();
            var specialPlatform = SpecialPlatform.Jollysports;
            var userName = _fixture.Create<string>();

            _appSettings.OfferCryptoCurrencyLicensees = new List<string>();
            _appSettings.CryptoLicenseesSupportBaseEur = new List<string>();
            _contextService.Setup(x => x.SiteName).Returns(licenseName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string>());

            object cacheValue = null;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            var service = CreateService();

            // Act
            var result = service.GetBaseCurrencyList();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBaseCurrencyList_WhenDefaultCase_ReturnsFlatCurrency()
        {
            // Arrange
            var licenseName = _fixture.Create<string>();
            var specialPlatform = _fixture.Create<int>();
            var userName = _fixture.Create<string>();

            _appSettings.OfferCryptoCurrencyLicensees = new List<string>();
            _appSettings.CryptoLicenseesSupportBaseEur = new List<string>();
            _contextService.Setup(x => x.SiteName).Returns(licenseName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string>());

            object cacheValue = null;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            var service = CreateService();

            // Act
            var result = service.GetBaseCurrencyList();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBaseCurrencyList_WhenSupportOnceCurrencyAndOneCurrencyNotBase_AddsNegativeCurrency()
        {
            // Arrange
            var licenseName = _fixture.Create<string>();
            var specialPlatform = _fixture.Create<int>();
            var userName = _fixture.Create<string>();
            var currencyList = new Dictionary<int, string> { { 123, "KRW" } };

            _appSettings.OfferCryptoCurrencyLicensees = new List<string>();
            _appSettings.CryptoLicenseesSupportBaseEur = new List<string>();
            _contextService.Setup(x => x.SiteName).Returns(licenseName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);

            object cacheValue = null;
            _cache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(false);

            var service = CreateService();

            // Act
            var result = service.GetBaseCurrencyList(true);

            // Assert
            Assert.Contains(-123, result.Keys);
            Assert.Equal("KRW", result[-123]);
        }

        private BaseCurrencyService CreateService()
        {
            return new BaseCurrencyService(_appSettings, _contextService.Object, _cache.Object);
        }
    }
}