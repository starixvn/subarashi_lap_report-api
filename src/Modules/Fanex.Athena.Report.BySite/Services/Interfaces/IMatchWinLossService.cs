using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface IMatchWinLossService
    {
        Task<IList<MatchWinLoss>> GetDataList(MatchWinLossMetaModel metaModel);

        Task<DataSet> GetDetailDataSet(MatchWinLossMetaModel metaModel);

        Task<IList<MatchWinLoss>> GetDetailDataList(MatchWinLossMetaModel metaModel);

        Task<IList<MatchWinLoss>> GetOutrightParlayList(MatchWinLossMetaModel metaModel);
    }
}
