namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;
    using Fanex.Athena.Report.Common.Model.DTO;

    public class PlatformWinLossMetaModel
    {
        public int CurrencyId { get; set; }

        public int PlatformType { get; set; }

        public int DrawFilter { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string AgentListXml { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public bool HasComm { get; set; }
    }
}