namespace Fanex.Athena.Report.BySite.DTOs
{
    public class CashOutWinLoss : BaseWinLoss
    {
        public string DateString { get; set; }

        public decimal Turnover { get; set; }

        public decimal LicenseeWL { get; set; }

        public decimal CustomerWL { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal BAComm { get; set; }

        public decimal AmtDueToBA { get; set; }

        public decimal BasedTurnover { get; set; }

        public decimal BasedLicenseeWL { get; set; }

        public decimal BasedCustomerWL { get; set; }

        public decimal BasedCustomerComm { get; set; }

        public decimal BasedBAComm { get; set; }

        public decimal BasedAmtDueToBA { get; set; }

        public int Currency { get; set; }

        public string CurrencyName { get; set; }

        public decimal CustomerTotal
        {
            get { return CustomerWL + CustomerComm; }
        }

        public decimal BasedCustomerTotal
        {
            get { return BasedCustomerWL + BasedCustomerComm; }
        }
    }
}