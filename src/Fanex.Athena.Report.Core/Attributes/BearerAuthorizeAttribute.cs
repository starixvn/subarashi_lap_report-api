namespace Fanex.Athena.Report.Core.Attributes
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class BearerAuthorizeAttribute : AuthorizeAttribute
    {
        public BearerAuthorizeAttribute(params string[] roles)
        {
            if (roles.Any())
            {
                Roles = string.Join(",", roles);
            }
            AuthenticationSchemes = "Bearer";
        }
    }
}