namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using System.Threading.Tasks;

    public interface IDailyBetsRepository
    {
        Task<(int totalRows, AuditTicket totalData)> GetTotalDataAsync(DailyBetsMetaModel metaModel);
    }
}