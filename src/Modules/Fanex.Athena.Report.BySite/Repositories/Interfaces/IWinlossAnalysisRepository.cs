using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.MetaModels;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IWinlossAnalysisRepository
    {
        Task<WinlossAnalysisAggregateDTO> GetDataBySettlementTimeCriteria(WinlossAnalysisMetaModel metaModel);

        Task<WinlossAnalysisAggregateDTO> GetDataByAgencyCriteria(WinlossAnalysisMetaModel metaModel);

        Task<WinlossAnalysisAggregateDTO> GetDataByPlayerCriteria(WinlossAnalysisMetaModel metaModel);
    }
}