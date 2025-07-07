namespace Fanex.Athena.Report.Common.Settings;

using System.Collections.Generic;

public class AppSettings
{
    public List<int> LicenseeAllowCasinoTransfer { get; set; }

    public string CustomerClassCategoryApi { get; set; }

    public bool IsShowAllDataForSettleMatchCountReport { get; set; }

    public List<string> OfferCryptoCurrencyLicensees { get; set; }

    public List<string> CryptoLicenseesSupportBaseEur { get; set; }

        public string CommonApiHostUrl { get; set; }

    public bool HasAmountDueToColumn { get; set; }

    public int MaxRowPerExcelFile { get; set; }
}