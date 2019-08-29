using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

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
            var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;

            if (error != null && error.Error != null)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                Fanex.Logging.Logger.Log.Error(error.Error.Message, error.Error);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    State = "Internal Server Error",
                    Msg = error.Error.Message
                }));
            }

            await _next.Invoke(context);
        }
    }
}