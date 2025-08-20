namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Model.DTO;
    using Common.Model.MetaModels;
    using Fanex.Core.Utils;

    public class CustomerBalanceMetaModel : IAgentMetaModel, IDataTestableMetaModel, IProductOptionsMetaModel, ICurrencyIdMetaModel, IValidationMetaModel
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public IEnumerable<ProductOption> ProductOptions { get; set; }

        public IEnumerable<ProductOption> OriginalProductOptions { get; set; }

        public int OrderItem { get; set; }

        public int TopCount { get; set; }

        public string AgentListXml { get; set; }

        public bool IsExcludedTestData { get; set; }

        public bool IsEnableSingleWallet { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }

        public bool HasComm { get; set; }

        public bool IsValid => FromDate <= ToDate && ProductOptions.Any(p => p.Status);

        public int CurrencyId { get; set; }
    }
}