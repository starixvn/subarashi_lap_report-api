using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Settings
{
    public class AppSettings
    {
        public string CustomerClassCategoryApi { get; set; }

        public bool IsShowAllDataForSettleMatchCountReport { get; set; }

        public string FunctionAllowedProductFilePath { get; set; }

        public List<string> OfferCryptoCurrencyLicensees { get; set; }

        public List<string> CryptoLicenseesSupportBaseEur { get; set; }

    }
}