using System;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class BaseWinLossMetaModel
    {
        public int DrawFilter { get; set; }

        public byte ReportBy { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string AgentListXml { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }
    }
}