namespace Fanex.Athena.Report.Core.Models.DTOs
{
    public class ShowingOption
    {
        public ShowingOption()
        {
            GrandTotalVisible = true;
            NGVisible = false;
            BAVisible = false;
            CSVisible = false;
            KNVisible = false;
            ABVisible = false;
        }

        public bool BAVisible { get; set; }

        public bool CSVisible { get; set; }

        public bool NGVisible { get; set; }

        public bool SBVisible { get; set; }

        public bool VSVisible { get; set; }

        public bool KNVisible { get; set; }

        public bool ABVisible { get; set; }

        public bool GrandTotalVisible { get; set; }

        public bool CustomerCommVisible { get; set; }
    }
}
