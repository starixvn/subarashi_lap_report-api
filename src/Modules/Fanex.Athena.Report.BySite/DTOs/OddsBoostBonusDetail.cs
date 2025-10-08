namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class OddsBoostBonusDetail
    {
        public DateTime Date { get; set; }

        public string DateString { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public string RefNo { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string PromotionType { get; set; }

        public decimal CustomerAmount { get; set; }

        public decimal BasedCustomerAmount { get; set; }
    }
}