using Fanex.Athena.Report.Core.Models.MetaModels;
using Fanex.Core.Utils;
using System.ComponentModel;

namespace Fanex.Athena.Report.BySite.MetaModels
{
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

        public bool IsAllowedBA { get; set; }
    }
}