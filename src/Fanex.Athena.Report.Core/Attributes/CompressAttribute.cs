using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO.Compression;
using System.Linq;

namespace Fanex.Athena.Report.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CompressFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var encodingsAccepted = context.HttpContext.Request.Headers["Accept-Encoding"];

            if (string.IsNullOrWhiteSpace(encodingsAccepted))
            {
                return;
            }

            encodingsAccepted = encodingsAccepted.ToString().ToLowerInvariant();
            var response = context.HttpContext.Response;

            if (encodingsAccepted.Contains("deflate"))
            {
                response.Headers.Add("Content-encoding", "deflate");
                response.Body = new DeflateStream(response.Body, CompressionMode.Compress);
            }

            if (encodingsAccepted.Contains("gzip"))
            {
                response.Headers.Add("Content-encoding", "gzip");
                response.Body = new GZipStream(response.Body, CompressionMode.Compress);
            }
        }
    }
}
