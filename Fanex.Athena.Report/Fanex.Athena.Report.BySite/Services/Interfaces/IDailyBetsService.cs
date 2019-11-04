namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface IDailyBetsService
    {
        Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel);
    }
}
