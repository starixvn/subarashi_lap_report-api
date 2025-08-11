namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using DTOs;
    using MetaModels;

    public interface IMatchWinLossRepository
    {
        Task<IList<MatchWinLoss>> GetDataList(MatchWinLossMetaModel metaModel);

        Task<DataSet> GetDetailDataSet(MatchWinLossMetaModel metaModel);

        Task<IList<MatchWinLoss>> GetDetailDataList(MatchWinLossMetaModel metaModel);

        Task<IList<MatchWinLoss>> GetOutrightParlayList(MatchWinLossMetaModel metaModel);
    }
}
