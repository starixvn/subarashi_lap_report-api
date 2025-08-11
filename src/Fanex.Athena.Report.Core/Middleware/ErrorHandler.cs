namespace Fanex.Athena.Report.Core.Middleware
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class ErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Features[typeof(IExceptionHandlerFeature)] is IExceptionHandlerFeature error && error.Error != null)
            {
                await WriteErrorMsgAsync(
                    context,
                    JsonConvert.SerializeObject(
                    new
                    {
                        Msg = "Internal Server Error",
                        Code = HttpStatusCode.InternalServerError
                    }));
            }
            else
            {
                try
                {
                    await _next.Invoke(context);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        private static async Task WriteErrorMsgAsync(HttpContext context, string jsonMessage)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonMessage);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogCritical(ex, ErrorMessageHelper.GenerateMessage(context, ex));

            var result = JsonConvert.SerializeObject(new
            {
                Msg = "Internal Server Error",
                Code = HttpStatusCode.InternalServerError
            });

            await WriteErrorMsgAsync(context, result);
        }
    }
}