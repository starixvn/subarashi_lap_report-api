using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class MatchWinLossMetaModel : ICurrencyIdMetaModel, IValidationMetaModel, IMetaModelProductCodeList,
                                         IProductOptionsMetaModel, IAgentMetaModel

    {
        private const int ThreeYearAgo = -3;

        public MatchWinLossMetaModel()
        {
            Date = DateTimeHelper.Now;
        }

        public int Level { get; set; }

        public string BalanceUpDay { get; set; }

        public string BaseCurrency { get; set; }

        public string ProductCode { get; set; }

        public int MatchId { get; set; }

        public int LeagueId { get; set; }

        public DateTime Date { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public virtual bool IsConvert => !string.IsNullOrEmpty(BaseCurrency);

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        public string AgentListXml { get; set; }

        public int CurrencyId { get; set; }

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public IEnumerable<ProductOption> ProductOptions { get; set; }

        public IEnumerable<ProductOption> OriginalProductOptions { get; set; }

        private bool IsValidBaseCurrency => string.IsNullOrEmpty(BaseCurrency) || SystemBaseCurrency.BaseCurrencyList
           .Any(x => x.Value.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase));

        public bool IsValidDetailRequest => !string.IsNullOrEmpty(ProductCodeList) && MatchId > 0 && LeagueId > 0
                                         && IsValidBaseCurrency;

        public bool IsValid => IsValidBaseCurrency && DateTimeHelper.Now.AddYears(ThreeYearAgo) <= Date;
    }
}
