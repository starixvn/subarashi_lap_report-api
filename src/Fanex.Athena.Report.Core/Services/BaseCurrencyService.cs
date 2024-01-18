using System;
using System.Collections.Generic;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Fanex.Athena.Report.Core.Services
{
    public class BaseCurrencyService : IBaseCurrencyService
    {
        private const string MemoryCacheBaseCurrencyListByLicensee = "MemoryCache_BaseCurrencyList_{0}_{1}_{2}_{3}";

        private readonly AppSettings _appSettings;

        private readonly IMemoryCache _cache;

        private readonly IContextService _contextService;

        public BaseCurrencyService(AppSettings appSettings, IContextService contextService, IMemoryCache cache)
        {
            _appSettings = appSettings;
            _contextService = contextService;
            _cache = cache;
        }

        public IDictionary<int, string> GetBaseCurrencyList(bool supportOnceCurrencyToBase = false)
        {
            var licenseeName = _contextService.SiteName;
            var specialPlatform = _contextService.SpecialPlatform;
            var userName = _contextService.UserName;
            var cacheKey = string.Format(MemoryCacheBaseCurrencyListByLicensee, licenseeName, specialPlatform, userName,
                supportOnceCurrencyToBase.ToString());
            var baseCurrencyList = _cache.Get<IDictionary<int, string>>(cacheKey);
            var currencyList = _contextService.CurrencyList;

            if (baseCurrencyList != null)
            {
                return baseCurrencyList;
            }

            baseCurrencyList = GetByLicenseeSettings(licenseeName, specialPlatform);

            if (supportOnceCurrencyToBase && HasOneCurrencyAndNotBase(currencyList, baseCurrencyList))
            {
                baseCurrencyList.Add(-currencyList.First().Key, currencyList.First().Value);
            }

            _cache.Set(cacheKey, baseCurrencyList);

            return baseCurrencyList;
        }

        private IDictionary<int, string> GetByLicenseeSettings(string licenseeName, int specialPlatform)
        {
            if (_appSettings.OfferCryptoCurrencyLicensees.Contains(licenseeName,
                    StringComparer.InvariantCultureIgnoreCase))
            {
                return SystemBaseCurrency.CryptoBaseCurrencyList;
            }

            if (_appSettings.CryptoLicenseesSupportBaseEur.Contains(licenseeName,
                    StringComparer.InvariantCultureIgnoreCase))
            {
                return SystemBaseCurrency.CryptoSupportEurBaseCurrencyList;
            }

            if (specialPlatform is SpecialPlatform.Jollysports or SpecialPlatform.JollysportCV)
            {
                return SystemBaseCurrency.JollySportsBaseCurrencyList;
            }

            return SystemBaseCurrency.FlatBaseCurrencyList;
        }

        private static bool HasOneCurrencyAndNotBase(IReadOnlyDictionary<int, string> currencyList,
                                                     IDictionary<int, string> baseCurrencyList)
            => currencyList.Count == 1 && !baseCurrencyList.Any(_ =>
                _.Value.Equals(currencyList.First().Value, StringComparison.OrdinalIgnoreCase));
    }
}
