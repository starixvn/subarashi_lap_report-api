namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    using Core.Models.MetaModels;
    using Fanex.Core.Utils;

    public class CustomerSupportTaskMetaModel : IAgentMetaModel, ISubsidiarySearchMetaModel, ICurrencyIdListMetaModel, IValidationMetaModel
    {
        private const int ReportTypeMaxValue = 3;

        public string AgentListXml { get; set; }

        public byte SupportTask { get; set; }

        public string CurrencyIdList { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(CurrencyIdList) && SupportTask >= 0 && SupportTask <= ReportTypeMaxValue;
    }
}