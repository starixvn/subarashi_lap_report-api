namespace Fanex.Athena.Report.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Data;
    using Microsoft.Extensions.Caching.Memory;

    public class CurrencyService : ICurrencyService
    {
        private const int CacheTimeoutHours = 24;

        private const string CacheKey = "REPORT_LAP_CACHE_CURRENCY";

        private readonly IMemoryCache _cache;

        public CurrencyService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public async Task<IEnumerable<Currency>> GetCurrencyListAsync()
        {
            var currencyList = _cache.Get<IEnumerable<Currency>>(CacheKey);

            if (currencyList != null)
            {
                return currencyList;
            }

            currencyList = await GetCurrencyListFromDbAsync().ConfigureAwait(false);
            _cache.Set(CacheKey, currencyList, DateTimeOffset.Now.AddHours(CacheTimeoutHours));

            return currencyList;
        }

        private static async Task<IEnumerable<Currency>> GetCurrencyListFromDbAsync()
        {
            const string UspName = "Lrf_CurrencySel";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                return await db.QueryAsync<Currency>().ConfigureAwait(false);
            }
        }
    }
}
