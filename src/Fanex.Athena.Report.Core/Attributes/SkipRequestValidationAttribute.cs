using System;

namespace Fanex.Athena.Report.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipRequestValidationAttribute : Attribute
    {
    }
}
