namespace Fanex.Athena.Report.BySite.Excels.Options
{
    using System.Collections.Generic;
    using MetaModels;

    public class WeeklyLiveMatchOptions
    {
        public WeeklyLiveMatchMetaModel MetaModel { get; set; }

        public IEnumerable<string> WeekHeaders { get; set; }
    }
}
