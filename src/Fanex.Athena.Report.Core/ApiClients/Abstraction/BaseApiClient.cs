using Fanex.Athena.Report.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.ApiClients.Abstraction
{
    public abstract class BaseApiClient
    {
        private const string LanguageKey = "Language";
        private const string JsonContentType = "application/json";

        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseApiClient(IHttpContextAccessor httpContextAccessor, System.Net.Http.HttpClient client)
        {
            Client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        protected System.Net.Http.HttpClient Client { get; }

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

        public virtual async Task<string> GetStringAsync(string path, int timeOut = 100)
        {
            using var cancelToken = new CancellationTokenSource(new TimeSpan(0, 0, timeOut));
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Client.BaseAddress + path));
            await PrepareHeaders(request.Headers).ConfigureAwait(false);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));

            HttpResponseMessage response = null;
            try
            {
                response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken.Token).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleUnSucessReponseAsync(response);
                }

                using (response)
                {
                    using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    using var streamReader = new StreamReader(stream);
                    using var textWriter = new StringWriter();
                    const int chunkLength = 4096;
                    var buffer = new char[chunkLength];
                    int readChunkLength;

                    do
                    {
                        readChunkLength = streamReader.ReadBlock(buffer, 0, chunkLength);
                        textWriter.Write(buffer, 0, readChunkLength);
                    } while (readChunkLength > 0);

                    return textWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException(ex.Message, ApiName, ex);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        protected virtual Task HandleUnSucessReponseAsync(HttpResponseMessage response)
        {
            response?.EnsureSuccessStatusCode();
            return Task.CompletedTask;
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