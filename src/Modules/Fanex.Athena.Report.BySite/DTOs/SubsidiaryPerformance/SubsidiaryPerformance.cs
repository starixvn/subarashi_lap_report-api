namespace Fanex.Athena.Report.BySite.DTOs.SubsidiaryPerformance
{
    public class SubsidiaryPerformance : SubsidiaryPerformanceBase
    {
        public int SubsidiaryId { get; set; }

        public int CurrencyId { get; set; }

        public string ProductCode { get; set; }

        public SubsidiaryPerformanceBase BaseCurrencyData { get; set; }
    }
}
