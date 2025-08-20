namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class BaseExtraPayout
    {
        public DateTime Date { get; set; }

        public int Index { get; set; }

        public int IndexRowSpan { get; set; }

        public int CurencyRowSpan { get; set; }

        public int ProductRowSpan { get; set; }

        public string DateString { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int BetCount { get; set; }

        public decimal Turnover { get; set; }

        public decimal Amount { get; set; }

        public decimal SharingAmount { get; set; }

        public decimal BasedTurnover { get; set; }

        public decimal BasedAmount { get; set; }

        public decimal BasedSharingAmount { get; set; }

        public int Currency { get; set; }

        public string CurrencyName { get; set; }
    }
}