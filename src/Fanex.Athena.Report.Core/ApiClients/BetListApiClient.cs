using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.ApiClients
{
    public class BetListApiClient : BaseApiClient, IBetListApiClient
    {
        private const string IgnoreShardingHeader = "X-IgnoreSharding";

        private const string SiteDbServerHeader = "X-SiteDBServer";

        private readonly IContextService _contextService;

        public BetListApiClient(IHttpContextAccessor httpContextAccessor, HttpClient client, IContextService contextService) : base(httpContextAccessor, client)
        {
            _contextService = contextService;
        }

        protected override string ApiName => "BETLIST";

        public async Task<byte[]> PostByteArrayAsync(string path, object value, string language)
        {
            return await HandleAsync(path, value, language, async response =>
                {
                    using (response)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                }
            ).ConfigureAwait(false);
        }

        protected override Task PrepareHeaders(HttpRequestHeaders headers)
        {
            headers.Add(IgnoreShardingHeader, _contextService.ForceIgnoreSharding.ToString());
            headers.Add(SiteDbServerHeader, _contextService.SiteDBServer);
            return Task.CompletedTask;
        }

    }
}
