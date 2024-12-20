namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using Fanex.Athena.Report.BySite.MetaModels;
    using System.Threading.Tasks;

    public interface IAbnormalBetService
    {
        Task<dynamic> GetDataAsync(AbnormalBetMetaModel metaModel);
    }
}