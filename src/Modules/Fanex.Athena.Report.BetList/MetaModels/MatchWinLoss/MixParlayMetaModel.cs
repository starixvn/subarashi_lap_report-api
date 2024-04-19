using System;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Core.Models;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BetList.MetaModels.MatchWinLoss
{
    public class MixParlayMetaModel : IValidationMetaModel, IAgentMetaModel, ICurrencyIdMetaModel,
                                      IMetaModelProductCodeList, ISubsidiarySearchMetaModel
    {
        public int BetType { get; set; }

        public string BaseCurrency { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public string AgentListXml { get; set; }

        public int CurrencyId { get; set; }

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        private bool IsValidBaseCurrency => string.IsNullOrEmpty(BaseCurrency) || SystemBaseCurrency.BaseCurrencyList
           .Any(x => x.Value.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase));

        public bool IsValid => FromDate <= ToDate && IsValidBaseCurrency;
    }
}
