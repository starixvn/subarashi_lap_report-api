using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface ITurnoverService
    {
        Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel);

        TurnoverDto SumTotalData(IList<TurnoverDto> dataList);
    }
}
