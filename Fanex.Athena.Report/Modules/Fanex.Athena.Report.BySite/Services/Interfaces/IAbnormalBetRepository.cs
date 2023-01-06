namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.MetaModels;

    public interface IAbnormalBetService
    {
        Task<dynamic> GetDataAsync(AbnormalBetMetaModel metaModel);
    }
}