namespace Fanex.Athena.Report.BySite.MetaModels
{
    using Core.Models.DTOs;

    public class SettledMatchCountMetaModel
    {
        public bool IsShowAll { get; set; }

        public int LicenseeId { get; set; }

        public ProductAccess ProductAccess { get; set; }
    }
}
