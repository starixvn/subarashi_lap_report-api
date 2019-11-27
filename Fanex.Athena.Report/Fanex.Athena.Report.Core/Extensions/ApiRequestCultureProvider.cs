using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Extensions
{
    public class ApiRequestCultureProvider : RequestCultureProvider
    {
        private const string LANGUAGEKEY = "Language";
        private const string DefaultCulture = "en-US";
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var culture = httpContext.Request.Headers[LANGUAGEKEY];

            if (culture.Count == 0 || string.IsNullOrEmpty(culture[0]))
            {
                return Task.FromResult(new ProviderCultureResult(DefaultCulture));
            }

            return Task.FromResult(new ProviderCultureResult(DefaultCulture, culture[0]));
        }
    }
}
