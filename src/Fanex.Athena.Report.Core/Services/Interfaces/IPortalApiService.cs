using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IPortalApiService
    {
        Task<IEnumerable<int>> GetSpecialLeaguesAsync(string licenseeName);
    }
}
