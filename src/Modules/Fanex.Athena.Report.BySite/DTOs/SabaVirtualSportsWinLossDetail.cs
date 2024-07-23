using System;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class SabaVirtualSportsWinLossDetail
    {
        public int Index { get; set; }

        public string SystemId { get; set; }

        public string ExternalId { get; set; }

        public int CustomerId { get; set; }

        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public DateTime Date { get; set; }

        public IDictionary<string, decimal> LicenseeWL { get; set; }

        public IDictionary<string, decimal> TurnOver { get; set; }

        public decimal TotalTransaction { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal CashOutCommission { get; set; }

        public decimal BACommission { get; set; }

        public decimal AmtDueToBA { get; set; }

        public decimal CustomerTotal { get; set; }
    }
}
