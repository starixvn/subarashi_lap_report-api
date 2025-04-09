using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface ISabaVirtualSportsWinLossRepository
    {
        Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetAgentWinlossDetailAsync(SabaVirtualSportsWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetCustomerWinlossDetailAsync(SabaVirtualSportsWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetSummaryWinlossAsync(SabaVirtualSportsWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);
    }
}
