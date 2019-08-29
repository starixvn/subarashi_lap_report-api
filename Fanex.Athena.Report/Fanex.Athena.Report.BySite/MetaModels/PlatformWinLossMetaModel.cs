using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class PlatformWinLossMetaModel
    {
        public object PlatformType { get; set; }

        public object MoveBetsDay { get; set; }

        public object DrawFilter { get; set; }

        public object FromDate { get; set; }

        public object ToDate { get; set; }

        public object AgentListXml { get; set; }

        public IEnumerable<string> Products { get; set; }
        public object BaseCurrency { get; internal set; }
    }
}
