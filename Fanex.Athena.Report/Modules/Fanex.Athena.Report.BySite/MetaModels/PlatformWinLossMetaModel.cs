namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Core.Models;
    using Fanex.Core.Utils;

    public class PlatformWinLossMetaModel : IProductOptionsMetaModel, IAgentMetaModel, IDataTestableMetaModel,
        ICurrencyIdMetaModel, ISubsidiarySearchMetaModel, IPlayerComMetaModel
    {
        public int CurrencyId { get; set; }

        public string PlatformType { get; set; }

        public int DrawFilter { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string AgentListXml { get; set; }

        public IEnumerable<ProductOption> ProductOptions { get; set; }

        public IEnumerable<ProductOption> OriginalProductOptions { get; set; }

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public bool HasPlayerCom { get; set; }

        public bool IsExcludedTestData { get; set; }

        public int Level { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }
    }
}