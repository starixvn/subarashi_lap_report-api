using System;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class SabaVirtualSportsWinLoss
    {
        public int Index { get; set; }

        public DateTime Date { get; set; }

        public int RowSpan { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public int CustomerId { get; set; }

        public string CurrencyName { get; set; }

        public int CurrencyId { get; set; }

        public decimal TotalTransaction { get; set; }

        public decimal CashOutCommission { get; set; }

        #region ICustomerWinLoss Members

        public decimal CustomerWinLoss { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal CustomerTotal => CustomerWinLoss + CustomerComm;

        #endregion ICustomerWinLoss Members

        #region ILicenseeWL Members

        public decimal LicenseeWLSB { get; set; }

        public decimal LicenseeWLCO { get; set; }

        public virtual decimal TotalLicenseeWL => LicenseeWLSB + LicenseeWLCO;

        public decimal TotalLicenseeWLSingle => TotalLicenseeWL;

        #endregion ILicenseeWL Members

        #region ITurnOver Members

        public decimal TurnOverSB { get; set; }

        public virtual decimal TotalTurnOver => TurnOverSB;

        public decimal TotalTurnOverSingle => TotalTurnOver;

        #endregion ITurnOver Members
    }
}
