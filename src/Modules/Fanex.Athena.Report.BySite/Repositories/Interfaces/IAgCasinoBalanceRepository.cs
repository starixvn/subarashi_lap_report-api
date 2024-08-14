using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IAgCasinoBalanceRepository
    {
        Task<SortableReportResultModel<AgCasinoBalance>> GetAgentBalanceDetail(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<SortableReportResultModel<AgCasinoBalance>> GetCustomerBalanceDetail(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<SortableReportResultModel<AgCasinoBalance>> GetSummaryBalance(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}
