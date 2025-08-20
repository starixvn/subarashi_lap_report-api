namespace Fanex.Athena.Report.BySite.Models
{
    using Common.Model.ViewModel;
    using DTOs;

    public class WeeklyLiveMatchResultModel : SortableResultModel<WeeklyLiveMatch>
    {
        public int[] WeeksTotalData { get; set; }

        public int NumberOfWeeks { get; set; }
    }
}
