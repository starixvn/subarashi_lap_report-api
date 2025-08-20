namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Extensions;
    using Core.Models.DTOs;
    using DTOs;
    using MetaModels;

    public interface ILiveReportRepository
    {
        Task<IList<LiveReport>> GetLiveReportSummary(LiveReportMetaModel metaModel);

        Task<IList<LiveReportBySport>> GetDetailBySport(LiveReportMetaModel metaModel, int[] liveReportNoDetailLinkSports);

        Task<Tuple<IList<LiveReport>, IList<LiveReportByBetType>>> GetDetailByCurrencyAndBettype(
            LiveReportMetaModel metaModel,
            IDictionary<int, string> currencyList,
            IEnumerable<ProductInfo> products);

        Task<SortableList<LiveReportWinLossByMatch>> GetLiveReportWinlossByMatch(LiveReportMatchListMetalModel metaModel);
    }
}