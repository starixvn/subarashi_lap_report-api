namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DTOs;
    using MetaModels;

    public interface ITurnoverService
    {
        Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel);

        TurnoverDto SumTotalData(IList<TurnoverDto> dataList);
    }
}
