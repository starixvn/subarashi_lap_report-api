namespace Fanex.Athena.Report.Common.Model.ViewModel
{
    using System.Collections.Generic;

    public class ReportResultModel<T>
    {
        public virtual IEnumerable<T> DataList { get; set; }

        public T TotalData { get; set; }

        public IEnumerable<T> TotalDataList { get; set; }

        public int TotalRows { get; set; }

        public IDictionary<string, T> SummaryTotal { get; set; }

        public string AdditionalInfo { get; set; }
    }
}