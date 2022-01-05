namespace Fanex.Athena.Report.BySite.DTOs
{
    public class ExtraPayout : BaseExtraPayout
    {
        public int CustId { get; set; }

        public string Username { get; set; }

        public string Nickname { get; set; }

        public int PromotionType { get; set; }

        public string PromotionName { get; set; }
    }
}