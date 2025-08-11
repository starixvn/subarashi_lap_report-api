namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface INewCashOutWinLossService
    {
        Task<SortableReportResultModel<CashOutWinLoss>> GetDataAsync(NewCashOutWinLossMetaModel metaModel);
    }
}