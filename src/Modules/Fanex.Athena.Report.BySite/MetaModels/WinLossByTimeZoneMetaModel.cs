namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using Common.Model.MetaModels;
    using Fanex.Core.Utils;
    using MetaModelHelper = Common.Helpers.MetaModelHelper;

    public class WinLossByTimeZoneMetaModel : IValidationMetaModel, ICurrencyIdMetaModel, IMetaModelProductCodeList,
        IAgentMetaModel
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string BaseCurrency { get; set; }

        public bool IsConvertAll { get; set; }

        public bool IsExcludedTestData { get; set; }

        public bool IsIncludeDrawData { get; set; }

        public int GroupBy { get; set; }

        public int ReportBy { get; set; }

        public int TimeZone { get; set; }

        public string CurrencyName { get; set; }
        public string AgentListXml { get; set; }

        public int CurrencyId { get; set; }

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public bool IsValid => FromDate <= ToDate && MetaModelHelper.ValidateBaseCurrency(BaseCurrency);
    }
}