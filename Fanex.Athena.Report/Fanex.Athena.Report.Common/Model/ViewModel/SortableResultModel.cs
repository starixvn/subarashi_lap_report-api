using System;
using System.Collections.Generic;
using System.Text;
using Fanex.Athena.Report.Common.Extensions;

namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    public class SortableResultModel<T> : ResultModel<T>
    {
        public new SortableList<T> DataList { get; set; }
    }
}
