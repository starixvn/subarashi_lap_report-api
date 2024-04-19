using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fanex.Athena.Report.Core.ApiClients.Abstraction
{
    public abstract class BaseApiClient
    {
        private const string LanguageKey = "Language";

        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            NullValueHandling = NullValueHandling.Ignore
        };


        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseApiClient(IHttpContextAccessor httpContextAccessor, HttpClient client)
        {
            Client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        protected HttpClient Client { get; }

        protected HttpContext Context => _httpContextAccessor.HttpContext;

        protected abstract string ApiName { get; }

        protected abstract Task PrepareHeaders(HttpRequestHeaders headers);

        public async Task<T> PostAsync<T>(string path, object value, string language)
        {
            return await HandleAsync(path, value, language, async response =>
            {
                using (response)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync();

                    using var streamReader = new StreamReader(stream);
                    using var jsonTextReader = new JsonTextReader(streamReader);

                    var serializer = JsonSerializer.Create(JsonSerializerSettings);
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }).ConfigureAwait(false);
        }

        public async Task<string> PostStringAsync(string path, object value, string language)
        {
            return await HandleAsync(path, value, language,
                async response =>
                {
                    using (response)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            ).ConfigureAwait(false);
        }

        protected async Task<T> HandleAsync<T>(string path, object value, string language,
                                               Func<HttpResponseMessage, Task<T>> codeBlock)
        {
            if (codeBlock == null)
            {
                return await Task.FromResult(default(T));
            }

            using var cancelToken = new CancellationTokenSource();
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(Client.BaseAddress + path))
            {
                Content = ToJsonContent(value)
            };

            await PrepareHeaders(request.Headers).ConfigureAwait(false);
            request.Headers.Add(LanguageKey, language);

            var result = await SendRequestAsync(request, cancelToken.Token).ConfigureAwait(false);

            return await codeBlock(result).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken token)
        {
            HttpResponseMessage response = null;

            try
            {
                response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (Exception ex)
            {
                response?.Dispose();

                throw new ServiceException(ex.Message, ApiName, ex);
            }
        }

        private static HttpContent ToJsonContent(object obj)
        {
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);

            var serializer = JsonSerializer.Create(JsonSerializerSettings);
            serializer.Serialize(jsonWriter, obj);
            return new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/json");
        }
    }
}
