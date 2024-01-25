using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    public interface ISubIssueService
    {
        IDictionary<int, string> GetSubIssueList();
    }
}