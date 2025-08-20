namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface INewCashOutWinLossRepository
    {
        Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMainData(
            NewCashOutWinLossMetaModel metaModel);

        Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMemberData(
            NewCashOutWinLossMetaModel metaModel);
    }
}