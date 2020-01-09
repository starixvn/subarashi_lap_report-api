namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    using System;
    using System.Collections.Generic;
    using Fanex.Athena.Report.Common.Model.DTO;

    public class CustomerBalanceMetaModel
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public int OrderItem { get; set; }

        public int TopCount { get; set; }

        public string AgentListXml { get; set; }

        public bool IsAllowedBA { get; set; }

        public bool HasComm { get; set; }

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }
    }
}