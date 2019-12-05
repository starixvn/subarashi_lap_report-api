namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using Fanex.Athena.Report.Common.Model.DataDomain;

    public class PromotionPrizeMetaModel
    {
        public int CurrencyId { get; set; }

        public String CurrencyName { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string UserName { get; set; }

        public string AgentListXml { get; set; }

        public int AgentId { get; set; }

        public string DefaultBaseCurrency { get; } = SystemBaseCurrency.EUR;
    }
}
