namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class OddsBoostBonusDetailResultModel
    {
        public List<OddsBoostBonusDetail> Data { get; set; }

        public OddsBoostBonusDetail TotalSummary { get; set; }
    }
}
