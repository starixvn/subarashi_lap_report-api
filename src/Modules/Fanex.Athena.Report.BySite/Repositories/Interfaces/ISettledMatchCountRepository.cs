using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface ISettledMatchCountRepository
    {
        Task<IList<SettledMatchCount>> GetDataList(bool isShowAll, int licenseeId);
    }
}
