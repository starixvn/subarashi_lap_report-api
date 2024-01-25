using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;

namespace Fanex.Athena.Report.BySite.Models
{
    public class NewCashOutWinLossResultModel
    {
        public List<CashOutWinLoss> Data { get; set; }

        public List<CashOutWinLoss> TotalCurrency { get; set; }

        public CashOutWinLoss TotalSummary { get; set; }

        public int TotalRows { get; set; }
    }
}