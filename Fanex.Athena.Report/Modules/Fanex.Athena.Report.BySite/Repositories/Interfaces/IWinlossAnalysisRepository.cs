using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.Params;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IWinlossAnalysisRepository
    {
        Task<WinlossAnalysisAggregateDTO>GetDataByAgencyCriteria(WinlossAnalysisParam metaModel);

        Task<WinlossAnalysisAggregateDTO> GetDataByPlayerCriteria(WinlossAnalysisParam metaModel);
    }
}
