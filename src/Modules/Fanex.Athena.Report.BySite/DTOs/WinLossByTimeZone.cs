namespace Fanex.Athena.Report.BySite.DTOs
{
    using System;

    public class WinLossByTimeZone
    {
        private const int OneHundredPercent = 100;

        public DateTime ReportBy { get; set; }

        public string GroupBy { get; set; }

        public int BetCount { get; set; }

        public decimal Turnover { get; set; }

        public decimal CustomerComm { get; set; }

        public decimal LicenseeWinLoss { get; set; }

        public decimal LicenseeComm { get; set; }

        public decimal LicenseeNetWinLoss => LicenseeWinLoss - CustomerComm + LicenseeComm;

        public decimal Margin => Turnover == 0 ? 0 : LicenseeNetWinLoss / Turnover * OneHundredPercent;
    }
}