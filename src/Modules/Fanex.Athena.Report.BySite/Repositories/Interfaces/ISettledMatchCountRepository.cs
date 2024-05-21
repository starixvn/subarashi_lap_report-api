using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface ISettledMatchCountRepository
    {
        Task<IList<SettledMatchCount>> GetDataList(bool isShowAll, int licenseeId);
    }
}
