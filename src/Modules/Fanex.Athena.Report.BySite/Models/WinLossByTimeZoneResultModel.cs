namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class WinLossByTimeZoneResultModel
    {
        public IEnumerable<WinLossByTimeZone> Data { get; set; }

        public WinLossByTimeZone Total { get; set; }
    }
}