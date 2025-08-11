namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System.ComponentModel;
    using Core.Models.MetaModels;
    using Fanex.Core.Utils;

    public class NewCashOutWinLossMetaModel : BaseWinLossMetaModel, ISubsidiarySearchMetaModel, ICurrencyIdListMetaModel
    {
        public int DrillDownLevel { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        public string CurrencyIdList { get; set; }

        public string SportIdList { get; set; }

        public bool IsConvertAllToBaseCurrency { get; set; }

        public bool IsExportExcel { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }
    }
}