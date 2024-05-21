using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface IWinlossAnalysisService
    {
        Task<WinlossAnalysisAggregateDTO> GetRawDataAsync(WinlossAnalysisMetaModel param);

        Task<WinlossAnalysisResultModel> GetDataAsync(WinlossAnalysisMetaModel param);
    }
}