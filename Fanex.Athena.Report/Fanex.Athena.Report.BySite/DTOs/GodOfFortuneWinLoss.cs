namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class GodOfFortuneWinLoss : BaseWinLoss
    {
        public int CustomerId { get; set; }

        public string CurrencyName { get; set; }

        public int CurrencyId { get; set; }

        public decimal TurnOver { get; set; }

        public decimal Discount { get; set; }

        public decimal CustomerWinLoss { get; set; }

        public decimal LicenseeWinLoss { get; set; }
    }
}