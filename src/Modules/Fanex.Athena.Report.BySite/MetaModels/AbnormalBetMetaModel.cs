using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Core.Models.MetaModels;
using Fanex.Core.Utils;
using System.ComponentModel;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class AbnormalBetMetaModel : BaseWinLossMetaModel, ICurrencyIdListMetaModel, ISpecialPlatformMetaModel
    {
        public int ViewIn { get; set; }

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [DefaultValue("")]
        public string BaseCurrency { get; set; }

        private string _userName;

        public string UserName
        {
            get => string.IsNullOrEmpty(_userName) ? string.Empty : _userName.Trim();

            set => _userName = value;
        }

        private string _leagueName;

        public string LeagueName
        {
            get => string.IsNullOrEmpty(_leagueName) ? string.Empty : _leagueName.Trim();

            set => _leagueName = value;
        }

        private string _licenseeName;

        public string LicenseeName
        {
            get => string.IsNullOrEmpty(_licenseeName) ? string.Empty : _licenseeName.Trim();

            set => _licenseeName = value;
        }

        private string _ip;

        public string Ip
        {
            get => string.IsNullOrEmpty(_ip) ? string.Empty : _ip.Trim();

            set => _ip = value;
        }

        public string BetTypeIdList { get; set; }

        public string StatusIdList { get; set; }

        public string SportIdList { get; set; }

        public string CurrencyIdList { get; set; }

        public string ReasonIdList { get; set; }

        public long RefNo { get; set; }

        public bool IsConvertAllToBaseCurrency { get; set; }

        public bool IsShowIpvn { get; set; }

        public int SpecialPlatform { get; set; }

        public bool IsHideSaba => SabaHelper.IsHideSaba(SpecialPlatform);
    }
}