namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IGodOfFortuneWinLossService
    {
        Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetDataAsync(GodOfFortuneWinLossMetaModel metaModel);
    }
}