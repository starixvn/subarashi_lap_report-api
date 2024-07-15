namespace Fanex.Athena.Report.BetList.MetaModels.PendingFunds
{
    using Interfaces;

    public class PendingFundsBetListMetaModel : BetListMetaModel, IRunningBetListMetaModel
    {
        public bool ExcludeFreeBet { get; set; }
    }
}