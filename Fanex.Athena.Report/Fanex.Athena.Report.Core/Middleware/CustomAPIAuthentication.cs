namespace Fanex.Athena.Report.Core.Middleware
{
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.Core.DTO;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    public class CustomApiAuthentication
    {
        private readonly RequestDelegate _next;
        private readonly ApplicationOptions _applicationOptions;

        public CustomApiAuthentication(RequestDelegate next, IOptions<ApplicationOptions> applicationOptionAccessor)
        {
            _next = next;
            _applicationOptions = applicationOptionAccessor.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var isIgnored = context.Request.Path.Value.StartsWith("/health") || context.Request.Path.Value.Contains("favicon.ico");
            if (!isIgnored)
            {
                var postedKey = context.Request.Headers["AUTH_TOKEN"];
                var isAuthorized = !string.IsNullOrEmpty(postedKey) && postedKey.Equals(_applicationOptions.AuthKey);
                if (!isAuthorized)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }
            await _next.Invoke(context);
        }
    }
}
