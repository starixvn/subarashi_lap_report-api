namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Model.DataDomain;
    using Common.Model.DTO;
    using Common.Model.MetaModels;
    using Fanex.Core.Utils;

    public class CustomerPendingFundsMetaModel : IValidationMetaModel, IAgentMetaModel, ICurrencyIdMetaModel, IProductOptionsMetaModel
    {
        private const int DefaultPageSize = 1000;

        private const int DefaultPageIndex = 1;

        public bool IsAscSort { get; set; } = true;

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public int OrderItem { get; set; }

        public int PageSize { get; set; } = DefaultPageSize;

        public int PageIndex { get; set; } = DefaultPageIndex;

        public bool IsConvert => SystemBaseCurrency.BaseCurrencyList.Any(t => t.Value.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase));

        public string AgentListXml { get; set; }

        public int CurrencyId { get; set; }

        public IEnumerable<ProductOption> ProductOptions { get; set; }

        public IEnumerable<ProductOption> OriginalProductOptions { get; set; }

        public bool IsValid => string.IsNullOrEmpty(BaseCurrency) || IsConvert;
    }
}