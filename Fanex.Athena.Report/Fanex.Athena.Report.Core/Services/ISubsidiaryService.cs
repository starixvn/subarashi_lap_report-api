using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Services
{
    public interface ISubsidiaryService
    {
        Task<IDictionary<int, int[]>> GetAgentList(string subsidiaryList);

        bool IsIgnoredSubsidiarySearch(bool subsidiarySearch, string subsidiaryList);
    }
}