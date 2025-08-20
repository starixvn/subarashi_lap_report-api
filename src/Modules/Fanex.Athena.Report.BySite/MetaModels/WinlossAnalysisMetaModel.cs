namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using Core.Models.MetaModels;
    using Enums;
    using Fanex.Core.Utils;

    public class WinlossAnalysisMetaModel : IAgentMetaModel, ISubsidiarySearchMetaModel, ICurrencyIdListMetaModel, IDataTestableMetaModel, IValidationMetaModel
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public DateTime MovebetDay { get; set; }

        public GroupByMode GroupMode { get; set; }

        public BreakdownMode BreakdownMode { get; set; }

        public ViewInTimeMode ViewInTime { get; set; }

        public string ProductCodes { get; set; }

        public string SportIds { get; set; }

        public string BetTypeIds { get; set; }

        public string PlatformIds { get; set; }

        public string StatusIds { get; set; }

        public string LiveStatusIds { get; set; }

        public string BaseCurrency { get; set; }

        public string BaseCurrencyName { get; set; }

        public bool IsVipOnly { get; set; }

        public string CustomerClass { get; set; }

        public string FullCustomerClass { get; set; }

        public string OddsGroup { get; set; }

        public string SearchUserName { get; set; }

        public int SearchMode { get; set; }

        public bool IsExcludedTestData { get; set; }

        public int CashOutMode { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string AgentListXml { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public string CurrencyIdList { get; set; }

        public string CurrencyCodes { get; set; }

        public WinlossAnalysisExportMode ExportExcelMode { get; set; }

        public int SortBy { get; set; }

        public int SortOrder { get; set; }

        public bool IsShowInOrginalCurrency { get; set; }

        public int SpecialPlatform { get; set; }

        public int SpecialPlatformType { get; set; }

        public string HourParts { get; set; }

        public bool IsValid => true;
    }
}