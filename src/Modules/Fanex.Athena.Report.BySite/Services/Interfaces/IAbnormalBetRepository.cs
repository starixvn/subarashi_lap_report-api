namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Threading.Tasks;
    using MetaModels;

    public interface IAbnormalBetService
    {
        Task<dynamic> GetDataAsync(AbnormalBetMetaModel metaModel);
    }
}