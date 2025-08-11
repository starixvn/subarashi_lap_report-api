namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IPlatformWinLossRepository
    {
        Task<SortableResultModel<PlatformWinLossMember>> GetMemberDataAsync(PlatformWinLossMetaModel metaModel);

        Task<ResultModel<PlatformWinLossSummary>> GetSummaryDataAsync(PlatformWinLossMetaModel metaModel);
    }
}