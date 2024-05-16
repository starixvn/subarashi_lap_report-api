using System;
using System.Collections.Generic;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.BySite.Models
{
    public class LiveReportViewModel
    {
        public string BaseCurrency { get; set; }

        public KeyValuePair<int, string>[] Currencies { get; set; }

        public KeyValuePair<int, string>[] BaseCurrencies { get; set; }

        public string CurrencyName { get; set; }

        public int CurrencyId { get; set; }

        public byte SubsidiaryViewType { get; set; }

        public bool IsAllowedCustomerCommission { get; set; }

        public bool HasAmountDueToColumn { get; set; }

        public bool IsAllowBa { get; set; }

        public bool IsAllowedNumberGame { get; set; }

        public DateTime BalanceUpDate { get; set; }

        public bool IsFakeLicensee { get; set; }

        public bool IsOneworksMaster { get; set; }

        public bool SubsidiarySearch { get; set; }

        public IEnumerable<string> SubsidiaryList { get; set; }

        public IEnumerable<CheckedItem> WhitelabelList { get; set; }
    }
}
