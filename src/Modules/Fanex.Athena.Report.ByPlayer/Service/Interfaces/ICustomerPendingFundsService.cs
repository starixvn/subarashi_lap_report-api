namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;

    public interface ICustomerPendingFundsService
    {
        Task<ReportResultModel<CustPendingFunds>> GetDataList(CustomerPendingFundsMetaModel metaModel);
    }
}