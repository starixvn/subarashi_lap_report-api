using System.Threading.Tasks;
using System.Collections.Generic;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface ISubsidiaryService
    {
        Task<IDictionary<int, int[]>> GetAgentList(string subsidiaryList);

        Task<IEnumerable<CheckedItem>> GetWhiteLabelTree(string licenseeName);

        bool IsIgnoredSubsidiarySearch(bool subsidiarySearch, string subsidiaryList);
    }
}