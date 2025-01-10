using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services
{
    public class WeeklyLiveMatchService : IWeeklyLiveMatchService
    {
        private const int DaysInWeek = 7;

        private readonly IWeeklyLiveMatchRepository _weeklyLiveMatchRepository;

        public WeeklyLiveMatchService(IWeeklyLiveMatchRepository weeklyLiveMatchRepository)
        {
            _weeklyLiveMatchRepository = weeklyLiveMatchRepository;
        }

        public async Task<WeeklyLiveMatchResultModel> GetData(WeeklyLiveMatchMetaModel metaModel) =>
            metaModel.IsValid ?
                await _weeklyLiveMatchRepository.GetDataList(metaModel).ConfigureAwait(false) :
                new WeeklyLiveMatchResultModel
                {
                    WeeksTotalData = Array.Empty<int>(),
                    DataList = new List<WeeklyLiveMatch>(),
                    NumberOfWeeks = 0
                };

        public IEnumerable<string> GetWeekHeaders(DateTime fromDate, DateTime toDate, int numberOfWeeks)
        {
            var culture = CultureInfo.GetCultureInfo(CultureInfo.CurrentUICulture.IetfLanguageTag);

            var firstDate = DateTimeHelper.GetFirstDateOfWeek(fromDate);
            var nextDate = firstDate.AddDays(DaysInWeek - 1);

            var headers = new List<string>
            {
                DateTimeHelper.GetMaxDate(firstDate, fromDate).ToString("dd MMM", culture) + " - " +
                DateTimeHelper.GetMinDate(nextDate, toDate).ToString("dd MMM yyyy", culture)
            };

            for (var i = 1; i < numberOfWeeks; i++)
            {
                firstDate = firstDate.AddDays(DaysInWeek);
                nextDate = firstDate.AddDays(DaysInWeek - 1);
                headers.Add(firstDate.ToString("dd MMM", culture) + " - " +
                            DateTimeHelper.GetMinDate(nextDate, toDate).ToString("dd MMM yyyy", culture));
            }

            return headers;
        }
    }
}
