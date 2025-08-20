namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using DTOs;
    using MetaModels;

    public interface IDailyBetsRepository
    {
        Task<(int totalRows, AuditTicket totalData)> GetTotalDataAsync(DailyBetsMetaModel metaModel);
    }
}