using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Middleware;
using Fanex.Athena.Report.Core.Models.DTOs.External;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fanex.Athena.Report.Core.Services
{
    public class PortalApiService : IPortalApiService
    {
        private const string DefaultLanguage = "en-US";

        private const int ThirtyMinutes = 30 * 60;

        private readonly IMemoryCache _cache;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILgApiClient _lgApiClient;

        private readonly ILogger _logger;

        public PortalApiService(ILgApiClient lgApiClient, IMemoryCache cache, ILogger<ErrorHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _lgApiClient = lgApiClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public Task<IEnumerable<int>> GetSpecialLeaguesAsync(string licenseeName)
        {
            const string path = "/Portal/GetSpecialLeagues";
            return TryGetOrAddFromCache($"SpecialLeagues_{licenseeName}", () => Handle(path, new { SiteName = licenseeName, }, Enumerable.Empty<int>()), ThirtyMinutes);
        }

        protected async Task<T> Handle<T>(string path, object payload, T defaultData)
        {
            try
            {
                var response = await _lgApiClient
                                    .PostAsync<PortalApiResponse<T>>(path, payload, DefaultLanguage)
                                    .ConfigureAwait(false);

                if (response.ErrorCode != 0)
                {
                    _logger.LogError($"An error occurred while calling LGAPI {path} - ErrorMsg: {response.ErrorMsg} - ErrorCode: {response.ErrorCode}");
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while calling LGAPI {path} - {ErrorMessageHelper.GenerateMessage(_httpContextAccessor.HttpContext, ex)}", ex);
                return defaultData;
            }
        }

        protected async Task<T> TryGetOrAddFromCache<T>(string key, Func<Task<T>> action, int timeout)
        {
            var result = _cache.Get<T>(key);

            if (!ReferenceEquals(result, default(T)))
            {
                return result;
            }

            result = await action.Invoke().ConfigureAwait(false);
            _cache.Set(key, result, DateTimeOffset.Now.AddSeconds(timeout));
            return result;
        }
    }
}
