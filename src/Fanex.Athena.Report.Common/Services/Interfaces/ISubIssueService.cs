namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using System.Collections.Generic;

    public interface ISubIssueService
    {
        IDictionary<int, string> GetSubIssueList();
    }
}