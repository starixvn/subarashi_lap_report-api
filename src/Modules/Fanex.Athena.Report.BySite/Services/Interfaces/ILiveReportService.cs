using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface ILiveReportService
    {
        Task<IList<LiveReport>> GetLiveReportSummary(LiveReportMetaModel model);

        Task<LiveReportDetailResultModel> GetLiveReportSummaryDetail(LiveReportMetaModel model, IDictionary<int, string> currencyList, IEnumerable<ProductInfo> products);

        Task<SortableList<LiveReportWinLossByMatch>> GetLiveReportWinLossByMatch(LiveReportMatchListMetalModel model);
    }
}
