using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class SettledMatchCountMetaModel
    {
        public bool IsShowAll { get; set; }

        public int LicenseeId { get; set; }

        public ProductAccess ProductAccess { get; set; }
    }
}
