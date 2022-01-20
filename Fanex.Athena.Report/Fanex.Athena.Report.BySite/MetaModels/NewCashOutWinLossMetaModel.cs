using System.Collections.Generic;
using System.ComponentModel;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class NewCashOutWinLossMetaModel : BaseWinLossMetaModel
    {
        public int DrillDownLevel { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        public IDictionary<int, string> UserCurrencyList { get; set; }

        public string DrillDownCurrencyName { get; set; }

        public string CurrencyIdList { get; set; }

        public bool IsConvertAllToBaseCurrency { get; set; }

        public bool IsExportExcel { get; set; }
    }
}