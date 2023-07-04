using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.MetaModels;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IWinlossAnalysisRepository
    {
        Task<WinlossAnalysisAggregateDTO>GetDataByAgencyCriteria(WinlossAnalysisMetaModel metaModel);

        Task<WinlossAnalysisAggregateDTO> GetDataByPlayerCriteria(WinlossAnalysisMetaModel metaModel);
    }
}
