using System.Net.Http;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Microsoft.AspNetCore.Http;

namespace Fanex.Athena.Report.Core.ApiClients
{
    public class LgApiClient : BaseApiClient, ILgApiClient
    {
        public LgApiClient(IHttpContextAccessor httpContextAccessor, HttpClient client) : base(httpContextAccessor, client) { }

        protected override string ApiName => "LG API";

    }
}
