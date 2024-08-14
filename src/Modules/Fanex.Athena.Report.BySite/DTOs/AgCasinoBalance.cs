namespace Fanex.Athena.Report.BySite.DTOs
{
    public class AgCasinoBalance : ThirdPartyWinLoss
    {
        public decimal OpeningBalance { get; set; }

        public decimal TransferIn { get; set; }

        public decimal TransferOut { get; set; }

        public decimal ClosingBalance { get; set; }
    }
}
