using Fanex.Athena.Report.Core.Models;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    public class CustomerSupportTaskMetaModel : IAgentMetaModel, ISubsidiarySearchMetaModel, ICurrencyIdListMetaModel, IValidationMetaModel
    {
        const int ReportTypeMaxValue = 3;

        public string AgentListXml { get; set; }
        public byte SupportTask { get; set; }
        public string CurrencyIdList { get; set; }
        public string SubsidiaryList { get; set; }
        public bool SubsidiarySearch { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(CurrencyIdList) && SupportTask >= 0 && SupportTask <= ReportTypeMaxValue;
    }
}
