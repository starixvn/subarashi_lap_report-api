namespace Fanex.Athena.Report.Common.Helpers
{
    using System;

    public static class DateTimeHelper
    {
        public static Func<DateTimeOffset> DateTimeOffsetProvider { get; set; } = () => DateTimeOffset.Now;

        public static DateTime Now => DateTimeOffsetProvider().DateTime;

        public static DateTime MinValue => DateTimeOffset.MinValue.DateTime;

        public static bool IsBetween(DateTime date, DateTime minDate, DateTime maxDate)
            => date.Date >= minDate.Date && date.Date <= maxDate.Date;

        public static DateTime GetMaxDate(DateTime dateOne, DateTime dateTwo) => dateOne > dateTwo ? dateOne : dateTwo;

        public static DateTime GetMinDate(DateTime dateOne, DateTime dateTwo) => dateOne < dateTwo ? dateOne : dateTwo;

        public static DateTime CreateDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
            => new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero).DateTime;

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