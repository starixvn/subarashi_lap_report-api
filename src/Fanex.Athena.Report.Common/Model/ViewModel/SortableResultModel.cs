namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using System.Collections.Generic;
    using Extensions;

    public class SortableResultModel<T> : ResultModel<T>
    {
        private ISortableList<T> _sortableDataList;

        public override IEnumerable<T> DataList
        {
            get => _sortableDataList;
            set => _sortableDataList = new SortableList<T>(value);
        }
    }
}