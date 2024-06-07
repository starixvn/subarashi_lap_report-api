using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class TurnoverDto
    {
        public int Index { get; set; }

        public string Date { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal Deposit { get; set; }

        public decimal Withdrawal { get; set; }

        public decimal Bonus { get; set; }

        public decimal Adjustment { get; set; }

        public decimal Promotion { get; set; }

        public decimal Turnover { get; set; }

        public decimal DrawGame { get; set; }

        public decimal NetTurnover { get; set; }

        public decimal PendingFunds { get; set; }

        [DataPermission(Permission = Roles.HidePlayerWL_WL_Comm_Total)]
        public decimal WinLoss { get; set; }

        public decimal ClosingBalance { get; set; }

        public decimal AffiliateTransfer { get; set; }

        public decimal Transfer3rd { get; set; }

        public decimal TurnoverBA { get; set; }

        public decimal CommBA { get; set; }

        public decimal AmountDueToBA { get; set; }

        [DataPermission(Permission = Roles.HidePlayerWL_WL_Comm_Total)]
        public decimal PlayerComm { get; set; }

        public decimal WinLossComm => WinLoss + PlayerComm;
    }
}
