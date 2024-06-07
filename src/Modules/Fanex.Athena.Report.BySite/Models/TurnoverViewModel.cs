using Fanex.Athena.Report.Common.Model.DTO;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class TurnoverViewModel
    {
        public string BaseCurrency { get; set; }

        public KeyValuePair<int, string>[] BaseCurrencies { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public KeyValuePair<int, string>[] Currencies { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }

        public bool IsAllowedCasinoTransfer { get; set; }

        public bool IsAllowBa { get; set; }

        public bool IsEnableSingleWallet { get; set; }

        public bool IsFakeLicensee { get; set; }

        public bool IsHidePlayerWl { get; set; }
    }
}
