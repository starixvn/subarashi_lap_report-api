namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using DTOs.WinlossAnalysis;
    using MetaModels;

    public interface IWinlossAnalysisRepository
    {
        Task<WinlossAnalysisAggregateDTO> GetDataBySettlementTimeCriteria(WinlossAnalysisMetaModel metaModel);

        Task<WinlossAnalysisAggregateDTO> GetDataByAgencyCriteria(WinlossAnalysisMetaModel metaModel);

        Task<WinlossAnalysisAggregateDTO> GetDataByPlayerCriteria(WinlossAnalysisMetaModel metaModel);
    }
}