namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Threading.Tasks;

    public interface IPlayerSabaCoinsService
    {
        Task<SortableReportResultModel<PlayerSabaCoins>> GetDataAsync(PlayerSabaCoinsMetaModel metaModel);
    }
}