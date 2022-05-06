namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class ExtraPayoutDetailMetaModel : ExtraPayoutMetaModel
    {
        public int CustId { get; set; }

        public string ExternalId { get; set; }

        public string SystemId { get; set; }
    }
}