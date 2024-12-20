namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using Fanex.Athena.Report.Common.Extensions;
    using System.Collections.Generic;

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