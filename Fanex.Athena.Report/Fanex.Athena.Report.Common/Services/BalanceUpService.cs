namespace Fanex.Athena.Report.Common.Services
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Data;
    using Microsoft.Extensions.Caching.Memory;

    public class BalanceUpService : IBalanceUpService
    {
        private const int CacheTimeout120Seconds = 120;

        private const string BalanceUpCacheKey = "EXT_LAP_CACHE_BALANCEUP";

        private readonly IMemoryCache _cache;

        public BalanceUpService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public async Task<BalanceUp> GetBalanceUpAsync()
        {
            var balanceup = _cache.Get<BalanceUp>(BalanceUpCacheKey);

            if (balanceup != null)
            {
                return balanceup;
            }

            balanceup = await GetBalanceUpDayFromDb();
            _cache.Set(BalanceUpCacheKey, balanceup, DateTimeOffset.Now.AddSeconds(CacheTimeout120Seconds));
            return balanceup;
        }

        private static async Task<BalanceUp> GetBalanceUpDayFromDb()
        {
            const string UspName = "Lrf_SelBalanceUpDay";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                var balanceUp = new BalanceUp();
                using (var reader = await db.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        balanceUp.BalanceUpDay = Convert.ToDateTime(reader["balanceupday"]);
                        balanceUp.MoveBetsDay = Convert.ToDateTime(reader["movebetsday"]);
                    }

                    return balanceUp;
                }
            }
        }
    }
}
