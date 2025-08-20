namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;

    public interface ICustomerWinLossService
    {
        Task<ResultModel<CustomerWinLoss>> GetDataList(CustomerWinLossMetaModel metaModel, bool checkPromotionPrize = false);
    }
}