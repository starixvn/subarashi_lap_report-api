using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface IAgCasinoBalanceService
    {
        Task<SortableReportResultModel<AgCasinoBalance>> GetData(ThirdPartyWinLossMetaModel metaModel,
                                                                 IReadOnlyDictionary<int, string> currencyList);
    }
}
