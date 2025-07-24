using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Fanex.Athena.Report.Core.ApiClients
{
    public class ApiResponse<T>
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public T Result { get; set; }
    }

    public class CommonApiClient : BaseApiClient, ICommonApiClient
    {
        protected override string ApiName => "Common API Client";
        private const string LanguageKey = "Language";

        private readonly IContextService _contextService;

        public CommonApiClient(IHttpContextAccessor httpContextAccessor, HttpClient client, IContextService contextService)
            : base(httpContextAccessor, client)
        {
            _contextService = contextService;
        }

        public async Task<T> GetAsync<T>(string path, object value = null, int timeOut = 100)
        {
            var queryString = value != null ? $"?{BuildQueryString(value)}" : string.Empty;
            var url = $"{path}{queryString}";

            var response = await GetStringAsync(url, timeOut).ConfigureAwait(false);

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(response);

            return apiResponse?.ErrorCode == 0 ? apiResponse.Result : default;
        }

        private static string BuildQueryString(object value)
        {
            var properties = value.GetType().GetProperties();
            var queryParams = new System.Collections.Specialized.NameValueCollection();

            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(value);
                if (propValue != null)
                {
                    queryParams.Add(prop.Name, propValue.ToString());
                }
            }

            return string.Join("&", Array.ConvertAll(queryParams.AllKeys, key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(queryParams[key])}"));
        }

        protected override Task PrepareHeaders(HttpRequestHeaders headers)
        {
            headers.Add(LanguageKey, _contextService.Language);

            return Task.CompletedTask;
        }
    }
}