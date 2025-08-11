namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using MetaModels;
    using Models;

    public interface IWeeklyLiveMatchRepository
    {
        Task<WeeklyLiveMatchResultModel> GetDataList(WeeklyLiveMatchMetaModel metaModel);
    }
}
