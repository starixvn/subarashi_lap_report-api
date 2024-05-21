using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;

namespace Fanex.Athena.Report.BySite.Repositories
{
    public class WeeklyLiveMatchRepository : IWeeklyLiveMatchRepository
    {
        private const int StartWeekColumnIndex = 2;

        private const string CustomSortingWeekColumn = "SortingWeekValue";

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public WeeklyLiveMatchRepository(
            ICacheRefAdapter cacheRefAdapter)
        {
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<WeeklyLiveMatchResultModel> GetDataList(WeeklyLiveMatchMetaModel metaModel)
        {
            const string uspName = "Lrf_Rpt_LiveMatchWeekly";

            using var db = ObjectDbFactory.CreateInstance(uspName);
            db.SetParameterValue("FromDate", metaModel.FromDate);
            db.SetParameterValue("ToDate", metaModel.ToDate);

            using var reader = await db.ExecuteReaderAsync();

            return ReadData(reader, metaModel.SortColumn, metaModel.SortDirection);
        }

        private static List<WeeklyLiveMatch> SortData(List<WeeklyLiveMatch> data, string sortColumn,
                                                      string sortDirection)
        {
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortDirection))
            {
                return data;
            }

            var isAscending = sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);
            var result = sortColumn.Equals(CustomSortingWeekColumn) ?
                Sort(data, x => x.SortingWeekValue, isAscending).ThenBy(x => x.SportName) :
                Sort(data, x => x.SportName, isAscending);

            AssignPropertyIndex(result);

            return result.ToList();
        }

        private static IOrderedEnumerable<T> Sort<T, TKey>(IEnumerable<T> data, Func<T, TKey> keySelector,
                                                           bool isAscending = true) =>
            isAscending ? data.OrderBy(keySelector) : data.OrderByDescending(keySelector);

        private static void AssignPropertyIndex(IEnumerable<WeeklyLiveMatch> data)
        {
            var index = 1;

            foreach (var item in data)
            {
                item.Index = index++;
            }
        }

        private WeeklyLiveMatchResultModel ReadData(
            IDataReader reader,
            string sortColumn,
            string sortDirection)
        {
            var data = new List<WeeklyLiveMatch>();
            var sortingWeekIndex = -1;
            var sortColumnName = sortColumn;

            if (!string.IsNullOrEmpty(sortColumn) && sortColumn.StartsWith("Week", StringComparison.OrdinalIgnoreCase))
            {
                sortingWeekIndex = int.Parse(sortColumn.Replace("Week", string.Empty));
                sortColumnName = CustomSortingWeekColumn;
            }

            var numberOfWeeks =
                reader.FieldCount <= StartWeekColumnIndex ? 0 : reader.FieldCount - StartWeekColumnIndex;
            var totalData = new int[numberOfWeeks];

            var index = 1;

            while (reader.Read())
            {
                var item = new WeeklyLiveMatch
                {
                    SportName = _cacheRefAdapter.GetSportName(reader["sporttype"]),
                    NumbersOfMatches = new List<int>()
                };
                var numbersOfMatches = new int[numberOfWeeks];

                for (var i = StartWeekColumnIndex; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? 0 : reader.GetInt32(i);
                    numbersOfMatches[i - StartWeekColumnIndex] = value;
                    totalData[i - StartWeekColumnIndex] += value;
                }

                item.NumbersOfMatches = numbersOfMatches;

                if (sortingWeekIndex >= 0 && sortingWeekIndex < numberOfWeeks)
                {
                    item.SortingWeekValue = numbersOfMatches[sortingWeekIndex];
                }

                item.Index = index++;
                data.Add(item);
            }

            data = SortData(data, sortColumnName, sortDirection);

            return new WeeklyLiveMatchResultModel
            {
                DataList = data,
                WeeksTotalData = totalData,
                NumberOfWeeks = numberOfWeeks
            };
        }
    }
}
