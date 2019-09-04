using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Middleware
{
    public class ErrorHandling
    {
        private readonly RequestDelegate _next;

        public ErrorHandling(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Features[typeof(IExceptionHandlerFeature)] is IExceptionHandlerFeature error && error.Error != null)
            {
                await WriteErrorMsgAsync(context, JsonConvert.SerializeObject(new
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
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonMessage);
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            Fanex.Logging.Logger.Log.Error(ex.Message, ex);

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