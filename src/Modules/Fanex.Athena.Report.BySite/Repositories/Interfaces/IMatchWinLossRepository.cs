using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IMatchWinLossRepository
    {
        Task<IList<MatchWinLoss>> GetDataList(MatchWinLossMetaModel metaModel);

        Task<DataSet> GetDetailDataSet(MatchWinLossMetaModel metaModel);

        Task<IList<MatchWinLoss>> GetDetailDataList(MatchWinLossMetaModel metaModel);

        Task<IList<MatchWinLoss>> GetOutrightParlayList(MatchWinLossMetaModel metaModel);
    }
}
