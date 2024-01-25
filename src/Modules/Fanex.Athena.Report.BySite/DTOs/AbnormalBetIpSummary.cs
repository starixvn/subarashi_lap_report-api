namespace Fanex.Athena.Report.BySite.DTOs
{
    public class AbnormalBetIpSummary
    {
        public string ExternalId { get; set; }

        public string SystemID { get; set; }

        public string LastLoginIP { get; set; }

        public string LastLoginLocation { get; set; }

        public string TicketIP { get; set; }

        public string TicketLocation { get; set; }
    }
}