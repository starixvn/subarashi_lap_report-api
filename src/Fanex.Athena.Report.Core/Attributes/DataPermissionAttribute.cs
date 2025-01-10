using System;

namespace Fanex.Athena.Report.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class DataPermissionAttribute : Attribute
    {
        public string Permission { get; set; }
    }
}
