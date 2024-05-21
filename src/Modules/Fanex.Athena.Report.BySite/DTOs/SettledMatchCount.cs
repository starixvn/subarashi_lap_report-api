using System;

namespace Fanex.Athena.Report.BySite.DTOs
{
    public class SettledMatchCount
    {
        public int IsToday { get; set; }

        public DateTime EventDate { get; set; }

        public int SportType { get; set; }

        public int Entered { get; set; }

        public int NonEntered { get; set; }
    }
}
