namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using System.Collections.Generic;
    using Fanex.Athena.Report.Common.Extensions;

    public class SortableResultModel<T> : ResultModel<T>
    {
        public SortableList<T> SortableDataList { get; set; }
        public override IEnumerable<T> DataList { get => this.SortableDataList; }
    }
}
