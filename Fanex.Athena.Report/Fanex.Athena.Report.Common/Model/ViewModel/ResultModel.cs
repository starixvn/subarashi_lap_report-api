namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using System.Collections.Generic;

    public class ResultModel<T>
    {
        public virtual IEnumerable<T> DataList { get; set; }

        public T TotalData { get; set; }

        public int TotalRows { get; set; }

        public string AdditionalInfo { get; set; }
    }
}