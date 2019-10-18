using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public interface ICustomerWinLossService
    {
        Task<ResultModel<CustomerWinLoss>> GetDataList(CustomerWinLossMetaModel metaModel);

        Task<IList<ResultModel<CustomerWinLoss>>> GetGroupResult(CustomerWinLossMetaModel metaModel, int rowPerGroup);
    }
}