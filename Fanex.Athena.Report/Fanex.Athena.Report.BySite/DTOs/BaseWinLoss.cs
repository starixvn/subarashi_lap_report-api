using System;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class BaseWinLoss
    {
        public int Index { get; set; }

        public DateTime Date { get; set; }

        public int CustId { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }

        public int TotalTransaction { get; set; }
    }
}