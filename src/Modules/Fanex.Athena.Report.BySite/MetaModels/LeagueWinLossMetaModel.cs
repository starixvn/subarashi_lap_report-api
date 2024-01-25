using System.ComponentModel;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Core.Utils;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class LeagueWinLossMetaModel : BaseWinLossMetaModel, ICurrencyIdListMetaModel, IMetaModelProductCodeList
    {
        public int LeagueId { get; set; }

        public int MatchId { get; set; }

        public int BettypeId { get; set; }

        public int EventType { get; set; }

        public string MatchName { get; set; } = string.Empty;

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public bool ShowBA { get; set; }

        public int DrillDownLevel { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        public string UserName { get; set; }

        private string _leagueName;

        public string LeagueName
        {
            get
            {
                return string.IsNullOrEmpty(_leagueName) ? string.Empty : _leagueName.Trim();
            }

            set
            {
                _leagueName = value;
            }
        }

        public string BetTypeIdList { get; set; }

        public string StatusIdList { get; set; }

        public string SportIdList { get; set; }

        public string CurrencyIdList { get; set; }

        public bool IsGroupInOriginalCurrency { get; set; }
    }
}