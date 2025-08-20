namespace Fanex.Athena.Report.BySite.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class WeeklyLiveMatchResponseModel
    {
        public IEnumerable<string> WeekHeaders { get; set; }

        public int NumberOfWeeks { get; set; }

        public CellModel[] Data { get; set; }

        public int[] WeeksTotalData { get; set; }
    }
}
