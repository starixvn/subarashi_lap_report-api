using Microsoft.AspNetCore.Http;
using System;

namespace Fanex.Athena.Report.Core.Extensions
{
    public static class HttpContextExtensions
    {
        public static T EnsureDispose<T>(this T disposable, HttpContext context) 
            where T : IDisposable
        {
            context.Response.RegisterForDispose(disposable);
            return disposable;
        }
    }
}
