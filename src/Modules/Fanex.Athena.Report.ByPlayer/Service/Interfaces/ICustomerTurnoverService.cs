using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public interface ICustomerTurnoverService
    {
        Task<ReportResultModel<CustTurnover>> GetDataList(CustomerTurnoverMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList, string languageCode);
    }
}
