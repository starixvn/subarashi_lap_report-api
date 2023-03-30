using Fanex.Athena.Report.BySite.Params;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface IWinlossAnalysisService
    {
        Task<object> GetData(WinlossAnalysisMetaModel param);
    }
}
