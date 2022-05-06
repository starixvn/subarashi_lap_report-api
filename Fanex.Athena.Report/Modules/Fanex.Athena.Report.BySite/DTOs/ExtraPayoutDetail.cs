namespace Fanex.Athena.Report.BySite.DTOs
{
    public class ExtraPayoutDetail : BaseExtraPayout
    {
        public string Refno { get; set; }

        public int PromotionType { get; set; }

        public string PromotionName { get; set; }
    }
}