namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;

    public interface ICustomerBalanceService
    {
        Task<ResultModel<CustomerBalance>> GetDataList(CustomerBalanceMetaModel metaModel);
    }
}