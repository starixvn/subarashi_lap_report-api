using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public interface ICustomerBalanceService
    {
        Task<ResultModel<CustomerBalance>> GetDataList(CustomerBalanceMetaModel metaModel);
    }
}