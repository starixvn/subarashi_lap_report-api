using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IWeeklyLiveMatchExcel
    {
        byte[] Export(string template, WeeklyLiveMatchResultModel data, WeeklyLiveMatchOptions options);
    }
}
