namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using AutoFixture.Xunit2;
    using Common.Constants;
    using Common.Model.DataDomain;
    using Common.Settings;
    using Core.Services;
    using Core.Services.Interfaces;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Internal;
    using Microsoft.Extensions.Options;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class BaseCurrencyServiceTest : IDisposable
    {
        private readonly AppSettings _appSettings;

        private readonly IMemoryCache _cache;

        private readonly Mock<IContextService> _contextService;

        public BaseCurrencyServiceTest()
        {
            _appSettings = FakeHelper.Create<AppSettings>();
            _contextService = new Mock<IContextService>();

            var cacheMock = new Mock<IOptions<MemoryCacheOptions>>();
            cacheMock.Setup(x => x.Value).Returns(new MemoryCacheOptions { Clock = new SystemClock() });
            _cache = new MemoryCache(cacheMock.Object);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_Cached_ReturnCache(
            string siteName,
            string userName,
            int specialPlatform,
            bool supportOnceCurrencyToBase,
            Dictionary<int, string> baseCurrencyList)
        {
            // Arrange
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            var cacheKey =
                $"MemoryCache_BaseCurrencyList_{siteName}_{specialPlatform}_{userName}_{supportOnceCurrencyToBase}";

            _cache.Set(cacheKey, baseCurrencyList);

            // Action
            var result = CreateService().GetBaseCurrencyList(supportOnceCurrencyToBase);

            // Assert
            Assert.Equal(baseCurrencyList, result);
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_WhenCall_ReturnDefaultData(
            string siteName,
            string userName,
            int specialPlatform,
            bool supportOnceCurrencyToBase,
            Dictionary<int, string> currencyList)
        {
            // Arrange
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);

            // Action
            var result = CreateService().GetBaseCurrencyList(supportOnceCurrencyToBase);

            // Assert
            Assert.Equal(SystemBaseCurrency.FlatBaseCurrencyList, result);
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_OfferCryptoCurrencyLicensees_ReturnCryptoBaseCurrencyList(
            string siteName,
            string userName,
            int specialPlatform,
            bool supportOnceCurrencyToBase,
            Dictionary<int, string> currencyList)
        {
            // Arrange
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);

            _appSettings.OfferCryptoCurrencyLicensees.Add(siteName);

            // Action
            var result = CreateService().GetBaseCurrencyList(supportOnceCurrencyToBase);

            // Assert
            Assert.Equal(SystemBaseCurrency.CryptoBaseCurrencyList, result);
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_CryptoLicenseesSupportBaseEur_ReturnCryptoSupportEurBaseCurrencyList(
            string siteName,
            string userName,
            int specialPlatform,
            bool supportOnceCurrencyToBase,
            Dictionary<int, string> currencyList)
        {
            // Arrange
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);

            _appSettings.CryptoLicenseesSupportBaseEur.Add(siteName);

            // Action
            var result = CreateService().GetBaseCurrencyList(supportOnceCurrencyToBase);

            // Assert
            Assert.Equal(SystemBaseCurrency.CryptoSupportEurBaseCurrencyList, result);
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_SpecialPlatformJollySports_ReturnJollySportsBaseCurrencyList(
            string siteName,
            string userName,
            bool supportOnceCurrencyToBase,
            Dictionary<int, string> currencyList)
        {
            // Arrange
            const int SpecialPlatformJollySports = SpecialPlatform.Jollysports;
            _contextService.Setup(x => x.SpecialPlatform).Returns(SpecialPlatformJollySports);

            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);

            // Action
            var result = CreateService().GetBaseCurrencyList(supportOnceCurrencyToBase);

            // Assert
            Assert.Equal(SystemBaseCurrency.JollySportsBaseCurrencyList, result);
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_SpecialPlatformJollySportCV_ReturnJollySportsBaseCurrencyList(
            string siteName,
            string userName,
            bool supportOnceCurrencyToBase,
            Dictionary<int, string> currencyList)
        {
            // Arrange
            const int SpecialPlatformJollySportCv = SpecialPlatform.JollysportCV;
            _contextService.Setup(x => x.SpecialPlatform).Returns(SpecialPlatformJollySportCv);

            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.UserName).Returns(userName);
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);

            // Action
            var result = CreateService().GetBaseCurrencyList(supportOnceCurrencyToBase);

            // Assert
            Assert.Equal(SystemBaseCurrency.JollySportsBaseCurrencyList, result);
        }

        [Theory]
        [AutoData]
        public void GetBaseCurrencyList_SupportOnceCurrencyToBase_ReturnJollySportsBaseCurrencyList(
            string siteName,
            string userName,
            int specialPlatform)
        {
            // Arrange
            const int ExpectedKey = 999999999;
            const string ExpectedValue = "Value";
            var currencyList = new Dictionary<int, string> { { ExpectedKey, ExpectedValue } };
            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _contextService.Setup(x => x.UserName).Returns(userName);

            // Action
            var result = CreateService().GetBaseCurrencyList(true);
            var exist = result.TryGetValue(-ExpectedKey, out var value);

            // Assert
            Assert.True(exist);
            Assert.Equal(ExpectedValue, value);
        }

        private BaseCurrencyService CreateService() => new(_appSettings, _contextService.Object, _cache);
    }
}