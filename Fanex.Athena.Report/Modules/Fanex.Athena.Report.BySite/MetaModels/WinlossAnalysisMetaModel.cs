using Fanex.Athena.Report.BySite.Enums;
using Fanex.Athena.Report.Core.Models;
using Fanex.Core.Utils;
using System;

namespace Fanex.Athena.Report.BySite.Params
{
    public class WinlossAnalysisMetaModel : IAgentMetaModel, ISubsidiarySearchMetaModel, ICurrencyIdListMetaModel
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateTime MovebetDay { get; set; }

        public GroupByMode GroupMode { get; set; }

        public BreakdownMode BreakdownMode { get; set; }

        public string ProductCodes { get; set; }

        public string SportIds { get; set; }

        public string BetTypeIds { get; set; }

        public string PlatformIds { get; set; }

        public string StatusIds { get; set; }

        public string LiveStatusIds { get; set; }

        public string BaseCurrency { get; set; }

        public bool IsVipOnly { get; set; }

        public string CustomerClass { get; set; }

        public string OddsGroup { get; set; }

        public int? SearchCustID { get; set; }

        public string SearchUserName { get; set; }

        public bool IsExludeTestData { get; set; }

        public bool IsViewCashOutOnly { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string AgentListXml { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public string CurrencyIdList { get; set; }
    }
}
