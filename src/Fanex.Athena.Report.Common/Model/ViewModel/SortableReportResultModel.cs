namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using Extensions;

    public class SortableReportResultModel<T> : ReportResultModel<T>
    {
        public new SortableList<T> DataList { get; set; }
    }
}