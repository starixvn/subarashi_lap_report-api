namespace Fanex.Athena.Report.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Common.Settings;
    using Helpers;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Middleware;
    using Models.DTOs;
    using Resources;

    public class CustomerClassService : ICustomerClassService
    {
        private const string MemoryCacheCustomerClassCategories = "MemoryCache_CustomerClassCategories";

        private readonly AppSettings _appSettings;

        private readonly IMemoryCache _cache;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger _logger;

        public CustomerClassService(IMemoryCache cache, ILogger<ErrorHandler> logger,
                                    IHttpContextAccessor httpContextAccessor, AppSettings appSettings,
                                    IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<RiskLevel>> GetAllRiskLevel()
        {
            var customerClassCategories = await GetCustomerClassCategories();

            customerClassCategories.Add(
                new CustomerClassCategory
                {
                    CustomerClass = 0,
                    CustomerClassName = RiskLevel.Uncategorized,
                    SabaRiskLevel = RiskLevel.Uncategorized
                }
            );

            var riskLevelList = customerClassCategories.GroupBy(_ => _.SabaRiskLevel).Select((_, index) => new RiskLevel
            {
                Id = index.ToString(),
                Key = _.Key,
                Name = ViewResources.ResourceManager.GetString(_.Key) ?? _.Key,
                CustomerClassList = _.OrderBy(cc => cc.CustomerClass).Select(cc => cc.CustomerClass.ToString())
                                     .Distinct()
            }).OrderBy(_ => _, new RiskLevelComparer());

            return riskLevelList;
        }

        private async Task<List<CustomerClassCategory>> GetCustomerClassCategories()
        {
            try
            {
                var customerClassCategory = _cache.Get<List<CustomerClassCategory>>(MemoryCacheCustomerClassCategories);

                if (customerClassCategory != null)
                {
                    return customerClassCategory;
                }

                var uri = new Uri(_appSettings.CustomerClassCategoryApi);

                using (var client = _httpClientFactory.CreateClient())
                {
                    var result = await client.GetFromJsonAsync<List<CustomerClassCategory>>(uri);

                    if (result is not null)
                    {
                        _cache.Set(MemoryCacheCustomerClassCategories, result);

                        var cacheOption = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
                        _cache.Set(MemoryCacheCustomerClassCategories + "_backup", result, cacheOption);

                        return result;
                    }
                }

                customerClassCategory =
                    _cache.Get<List<CustomerClassCategory>>(MemoryCacheCustomerClassCategories + "_backup");

                return customerClassCategory ?? new List<CustomerClassCategory>();
            }
            catch (Exception exception)
            {
                _logger.LogError(ErrorMessageHelper.GenerateMessage(_httpContextAccessor.HttpContext, exception));
            }

            return new List<CustomerClassCategory>();
        }
    }
}
