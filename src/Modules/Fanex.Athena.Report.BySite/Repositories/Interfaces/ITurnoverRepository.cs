namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DTOs;
    using MetaModels;

    public interface ITurnoverRepository
    {
        Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel);
    }
}
