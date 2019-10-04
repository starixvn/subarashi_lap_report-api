namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using System.Collections.Generic;
    using Fanex.Athena.Report.Common.Extensions;

    public class SortableResultModel<T> : ResultModel<T>
    {
        private ISortableList<T> _sortableDataList;
        public override IEnumerable<T> DataList
        {
            get => this._sortableDataList;
            set => _sortableDataList = new SortableList<T>(value);
        }
    }
}
