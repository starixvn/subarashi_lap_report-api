using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace Fanex.Athena.Report.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
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