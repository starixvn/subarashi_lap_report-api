namespace Fanex.Athena.Report.Core.Extensions
{
    using Fanex.Athena.Report.Core.Middleware;
    using Microsoft.AspNetCore.Builder;

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandling>();
        }

        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPFilter>();
        }

        public static IApplicationBuilder UseCustomApiAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomApiAuthentication>();
        }
    }
}