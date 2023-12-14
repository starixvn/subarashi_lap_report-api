using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface IWeeklyLiveMatchService
    {
        Task<WeeklyLiveMatchResultModel> GetData(WeeklyLiveMatchMetaModel metaModel);

        IEnumerable<string> GetWeekHeaders(DateTime fromDate, DateTime toDate, int numberOfWeeks);
    }
}
