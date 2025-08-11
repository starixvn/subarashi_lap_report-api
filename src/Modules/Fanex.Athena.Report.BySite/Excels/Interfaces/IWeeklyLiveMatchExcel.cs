namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Models;
    using Options;

    public interface IWeeklyLiveMatchExcel
    {
        byte[] Export(string template, WeeklyLiveMatchResultModel data, WeeklyLiveMatchOptions options);
    }
}
