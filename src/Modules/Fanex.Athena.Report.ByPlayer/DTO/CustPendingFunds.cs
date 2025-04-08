namespace Fanex.Athena.Report.ByPlayer.DTO
{
    public class CustPendingFunds
    {
        public int Index { get; set; }

        public int CustId { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public int BetCounts { get; set; }

        public decimal PendingFunds { get; set; }
    }
}