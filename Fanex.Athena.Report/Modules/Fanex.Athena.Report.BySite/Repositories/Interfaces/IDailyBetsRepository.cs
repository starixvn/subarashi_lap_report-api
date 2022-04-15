namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;

    public interface IDailyBetsRepository
    {
        Task<(int totalRows, AuditTicket totalData)> GetTotalDataAsync(DailyBetsMetaModel metaModel);
    }
}
