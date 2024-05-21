using System;

namespace Fanex.Athena.Report.Core.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetMaxDate(DateTime dateOne, DateTime dateTwo) => dateOne > dateTwo ? dateOne : dateTwo;

        public static DateTime GetMinDate(DateTime dateOne, DateTime dateTwo) => dateOne < dateTwo ? dateOne : dateTwo;

        public static DateTime GetFirstDateOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            const int DaysInAWeek = 7;

            var diff = date.DayOfWeek - startOfWeek;

            if (diff < 0)
            {
                diff += DaysInAWeek;
            }

            return date.AddDays(-1 * diff).Date;
        }
    }
}
