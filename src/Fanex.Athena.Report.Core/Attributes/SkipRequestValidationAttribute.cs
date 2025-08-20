namespace Fanex.Athena.Report.Core.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class SkipRequestValidationAttribute : Attribute
    {
    }
}
