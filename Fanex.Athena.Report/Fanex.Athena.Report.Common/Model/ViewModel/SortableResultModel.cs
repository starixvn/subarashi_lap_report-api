namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using Fanex.Athena.Report.Common.Extensions;

    public class SortableResultModel<T> : ResultModel<T>
    {
        public new SortableList<T> DataList { get; set; }
    }
}
