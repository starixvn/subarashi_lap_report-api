namespace Fanex.Athena.Report.Core.Models.DTOs
{
    public class ShowingOption
    {
        public ShowingOption()
        {
            GrandTotalVisible = true;
            NGVisible = false;
            BAVisible = false;
            KNVisible = false;
        }

        public bool BAVisible { get; set; }

        public bool NGVisible { get; set; }

        public bool SBVisible { get; set; }

        public bool VSVisible { get; set; }

        public bool KNVisible { get; set; }

        public bool GrandTotalVisible { get; set; }

        public bool CustomerCommVisible { get; set; }
    }
}
