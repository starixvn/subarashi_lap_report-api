namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class NewCashOutWinLossResultModel
    {
        public List<CashOutWinLoss> Data { get; set; }

        public List<CashOutWinLoss> TotalCurrency { get; set; }

        public CashOutWinLoss TotalSummary { get; set; }

        public int TotalRows { get; set; }
    }
}