namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using Common.Helpers;

    public class WeeklyLiveMatchMetaModel
    {
        public WeeklyLiveMatchMetaModel()
        {
            ToDate = DateTime.Now.AddDays(-1);
            FromDate = DateTimeHelper.GetFirstDateOfWeek(ToDate);
        }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string SortDirection { get; set; }

        public string SortColumn { get; set; }

        public bool IsValid => FromDate <= ToDate;

        public string BaseCurrency { get; set; }
    }
}
