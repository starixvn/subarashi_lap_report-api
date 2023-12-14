using System.Collections.Generic;
using Fanex.Athena.Report.BySite.MetaModels;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public class WeeklyLiveMatchOptions
    {
        public WeeklyLiveMatchMetaModel MetaModel { get; set; }

        public IEnumerable<string> WeekHeaders { get; set; }
    }
}
