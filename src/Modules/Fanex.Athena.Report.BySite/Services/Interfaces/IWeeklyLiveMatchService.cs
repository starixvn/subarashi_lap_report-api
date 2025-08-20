namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MetaModels;
    using Models;

    public interface IWeeklyLiveMatchService
    {
        Task<WeeklyLiveMatchResultModel> GetData(WeeklyLiveMatchMetaModel metaModel);

        IEnumerable<string> GetWeekHeaders(DateTime fromDate, DateTime toDate, int numberOfWeeks);
    }
}
