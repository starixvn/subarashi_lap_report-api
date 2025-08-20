namespace Fanex.Athena.Report.Core.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DataPermissionAttribute : Attribute
    {
        public string Permission { get; set; }
    }
}
