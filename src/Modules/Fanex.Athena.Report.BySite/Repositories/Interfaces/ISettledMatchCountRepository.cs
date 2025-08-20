namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DTOs;

    public interface ISettledMatchCountRepository
    {
        Task<IList<SettledMatchCount>> GetDataList(bool isShowAll, int licenseeId);
    }
}
