namespace Fanex.Athena.Report.BySite.DTOs
{
    public class CurrencyTotal
    {
        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public decimal SumAmount { get; set; }

        public decimal SumContributionAmount { get; set; }

        public decimal SumTipSharing { get; set; }

        public decimal SumTurnover { get; set; }
    }
}