using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Settings
{
    public class AppSettings
    {
        public string Environment { get; set; }

        public List<int> LicenseeAllowCasinoTransfer { get; set; }

        public string CustomerClassCategoryApi { get; set; }

        public bool IsShowAllDataForSettleMatchCountReport { get; set; }

        public string FunctionAllowedProductFilePath { get; set; }

        public List<string> OfferCryptoCurrencyLicensees { get; set; }

        public List<string> CryptoLicenseesSupportBaseEur { get; set; }

        public string LgApiTokenValue { get; set; }

        public string LgApiHostUrl { get; set; }

        public string BetListApiHostUrl { get; set; }

        public string BetListApiTokenValue { get; set; }

        public bool HasAmountDueToColumn { get; set; }

        public int MaxRowPerExcelFile { get; set; }
    }
}