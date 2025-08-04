namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Core.Models.DTOs;
    using DTOs;
    using MetaModels;
    using Models;

    public interface ILiveReportService
    {
        Task<IList<LiveReport>> GetLiveReportSummary(LiveReportMetaModel model);

        Task<LiveReportDetailResultModel> GetLiveReportSummaryDetail(
            LiveReportMetaModel model,
            IDictionary<int, string> currencyList,
            IEnumerable<ProductInfo> products,
            int[] liveReportNoDetailLinkSports);

        Task<SortableList<LiveReportWinLossByMatch>> GetLiveReportWinLossByMatch(LiveReportMatchListMetalModel model);
    }
}