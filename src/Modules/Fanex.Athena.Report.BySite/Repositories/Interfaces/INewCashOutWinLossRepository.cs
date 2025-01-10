namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Threading.Tasks;

    public interface INewCashOutWinLossRepository
    {
        Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMainData(
            NewCashOutWinLossMetaModel metaModel);

        Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMemberData(
            NewCashOutWinLossMetaModel metaModel);
    }
}