namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using DTOs.WinlossAnalysis;
    using MetaModels;
    using Models;

    public interface IWinlossAnalysisService
    {
        Task<WinlossAnalysisAggregateDTO> GetRawDataAsync(WinlossAnalysisMetaModel param);

        Task<WinlossAnalysisResultModel> GetDataAsync(WinlossAnalysisMetaModel param);
    }
}