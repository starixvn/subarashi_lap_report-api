using System.ComponentModel;
using Fanex.Athena.Report.Core.Models;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class ExtraPayoutMetaModel : BaseWinLossMetaModel, ICurrencyIdListMetaModel, ISubsidiarySearchMetaModel
    {
        private string _username;

        public string PromotionTypeIdList { get; set; }

        public string ProductCodeList { get; set; }

        public int DrillDownLevel { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        public string DrillDownCurrencyName { get; set; }

        public string Username
        {
            get
            {
                return string.IsNullOrEmpty(_username) ? string.Empty : _username?.Trim();
            }

            set
            {
                _username = value;
            }
        }

        public string CurrencyIdList { get; set; }

        public bool IsConvertAllToBaseCurrency { get; set; }

        public bool IsExportExcel { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }
    }
}