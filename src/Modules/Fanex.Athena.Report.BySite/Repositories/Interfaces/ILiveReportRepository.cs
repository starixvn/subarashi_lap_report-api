using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface ILiveReportRepository
    {
        Task<IList<LiveReport>> GetLiveReportSummary(LiveReportMetaModel metaModel);

        Task<IList<LiveReportBySport>> GetDetailBySport(LiveReportMetaModel metaModel);

        Task<Tuple<IList<LiveReport>, IList<LiveReportByBetType>>> GetDetailByCurrencyAndBettype(
            LiveReportMetaModel metaModel,
            IDictionary<int, string> currencyList,
            IEnumerable<ProductInfo> products);

        Task<SortableList<LiveReportWinLossByMatch>> GetLiveReportWinlossByMatch(LiveReportMatchListMetalModel metaModel);
    }
}
