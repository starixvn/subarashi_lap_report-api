using System;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class BaseWinLossMetaModel : IAgentMetaModel, IDataTestableMetaModel, IValidationMetaModel
    {
        public int DrawFilter { get; set; }

        public byte ReportBy { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateTime DrillDownFromDate { get; set; }

        public DateTime DrillDownToDate { get; set; }

        public string AgentListXml { get; set; }

        public bool IsExcludedTestData { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public virtual bool IsValid => FromDate <= ToDate && DrillDownFromDate <= DrillDownToDate;
    }
}