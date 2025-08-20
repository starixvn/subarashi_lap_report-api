namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using Common.Model.DTO;

    public class TurnoverViewModel
    {
        public string BaseCurrency { get; set; }

        public KeyValuePair<int, string>[] BaseCurrencies { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public KeyValuePair<int, string>[] Currencies { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }

        public bool IsAllowedCasinoTransfer { get; set; }

        public bool IsEnableSingleWallet { get; set; }

        public bool IsFakeLicensee { get; set; }

        public bool IsHidePlayerWl { get; set; }
    }
}