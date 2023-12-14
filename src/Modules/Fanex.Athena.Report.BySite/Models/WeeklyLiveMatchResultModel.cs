using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Models
{
    public class WeeklyLiveMatchResultModel : SortableResultModel<WeeklyLiveMatch>
    {
        public int[] WeeksTotalData { get; set; }

        public int NumberOfWeeks { get; set; }
    }
}
