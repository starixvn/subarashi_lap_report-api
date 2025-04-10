using System;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class SubsidiaryPerformanceMetaModel : IValidationMetaModel, IMetaModelProductCodeList, IAgentMetaModel
    {
        public string AgentListXml { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public ReportLevel DrillDownLevel { get; set; }

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public string SubsidiaryList { get; set; } = string.Empty;

        public string WhiteLabelList { get; set; } = string.Empty;

        public string SubsidiaryId { get; set; }

        public string BaseCurrency { get; set; }

        public bool ConvertAll { get; set; }

        public string LicenseeName { get; set; }

        public bool IncludeRaw { get; set; }

        public bool IsExcludeTest { get; set; }

        public bool IsSeparateSabaWinLoss { get; set; }

        public bool IsHideSaba { get; set; }

        public bool IsValid => FromDate <= ToDate && !string.IsNullOrEmpty(ProductCodeList) && !string.IsNullOrEmpty(BaseCurrency);
    }
}
