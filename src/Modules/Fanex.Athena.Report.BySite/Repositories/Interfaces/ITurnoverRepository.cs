using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface ITurnoverRepository
    {
        Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel);
    }
}
