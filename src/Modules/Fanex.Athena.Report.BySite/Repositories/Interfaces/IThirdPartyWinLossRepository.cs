using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IThirdPartyWinLossRepository<T>
    {
        Task<ReportResultModel<T>> GetAgentWinlossDetail(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string spname);

        Task<ReportResultModel<T>> GetCustomerWinlossDetail(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string spname);

        Task<ReportResultModel<T>> GetSummaryWinloss(ThirdPartyWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string spname);
    }
}
