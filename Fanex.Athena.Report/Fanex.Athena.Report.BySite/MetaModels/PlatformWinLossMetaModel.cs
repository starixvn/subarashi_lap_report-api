namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;

    public class PlatformWinLossMetaModel
    {
        public int CurrencyId { get; set; }

        public int PlatformType { get; set; }

        public DateTime MoveBetsDay { get; set; }

        public int DrawFilter { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string AgentListXml { get; set; }

        public IEnumerable<string> Products { get; set; }

        public string BaseCurrency { get; set; }
    }
}
