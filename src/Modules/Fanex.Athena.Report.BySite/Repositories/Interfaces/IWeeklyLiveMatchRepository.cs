using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    public interface IWeeklyLiveMatchRepository
    {
        Task<WeeklyLiveMatchResultModel> GetDataList(WeeklyLiveMatchMetaModel metaModel);
    }
}
