namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using Fanex.Athena.Report.Common.Extensions;

    public class SortableReportResultModel<T> : ReportResultModel<T>
    {
        public new SortableList<T> DataList { get; set; }
    }
}