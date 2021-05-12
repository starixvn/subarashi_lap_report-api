using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class GodOfFortuneWinLossMetaModel : BaseWinLossMetaModel
    {
        public int DrillDownCustomerId { get; set; }

        public int DrillDownLevel { get; set; }

        public DateTime DrillDownFromDate { get; set; }

        public DateTime DrillDownToDate { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        public IDictionary<int, string> UserCurrencyList { get; set; }

        public bool IsConvertAll { get; set; }

        public string DrillDownCurrencyName { get; set; }
    }
}