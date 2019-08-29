namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;

    public class ResultModel<T>
    {
        public virtual IEnumerable<T> DataList { get; set; }

        public T TotalData { get; set; }
    }
}
