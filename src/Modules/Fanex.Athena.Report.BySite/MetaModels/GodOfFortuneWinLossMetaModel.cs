namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System.ComponentModel;
    using Fanex.Core.Utils;

    public class GodOfFortuneWinLossMetaModel : BaseWinLossMetaModel, ICurrencyIdListMetaModel
    {
        public int DrillDownCustomerId { get; set; }

        public int DrillDownLevel { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        public bool IsConvertAll { get; set; }

        public string CurrencyIdList { get; set; }
    }
}