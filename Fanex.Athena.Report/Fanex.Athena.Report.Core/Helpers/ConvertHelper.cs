using System;

namespace Fanex.Athena.Report.Core.Helpers
{
    public static class ConvertHelper
    {
        public static DateTime ToDateTime(string date, bool isMonthly)
        {
            var year = Convert.ToInt32(date.Substring(0, 4));
            var month = Convert.ToInt32(date.Substring(4, 2));
            var day = isMonthly ? 1 : Convert.ToInt32(date.Substring(6, 2));

            return new DateTime(year, month, day);
        }
    }
}