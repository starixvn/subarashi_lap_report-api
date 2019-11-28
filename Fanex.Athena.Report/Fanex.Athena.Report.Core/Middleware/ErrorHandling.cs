namespace Fanex.Athena.Report.Core.Middleware
{
    using Fanex.Athena.Report.Core.Helpers;

    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class ErrorHandling
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public ErrorHandling(RequestDelegate next, ILogger<ErrorHandling> logger)
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
                        State = "Internal Server Error",
                        Msg = error.Error.Message
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
                State = "Internal Server Error",
                Msg = ex.Message,
                Code = HttpStatusCode.InternalServerError
            });

            await WriteErrorMsgAsync(context, result);
        }
    }
}