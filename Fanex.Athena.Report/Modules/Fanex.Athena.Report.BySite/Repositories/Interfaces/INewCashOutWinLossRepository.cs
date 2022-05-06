namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface INewCashOutWinLossRepository
    {
        Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMainData(
            NewCashOutWinLossMetaModel metaModel);

        Task<SortableReportResultModel<CashOutWinLoss>> GetNewCashOutWinLossMemberData(
            NewCashOutWinLossMetaModel metaModel);
    }
}