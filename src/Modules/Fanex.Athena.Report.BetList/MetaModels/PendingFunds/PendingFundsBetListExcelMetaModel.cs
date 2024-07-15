namespace Fanex.Athena.Report.BetList.MetaModels.PendingFunds
{
    using Interfaces;

    public class PendingFundsBetListExcelMetaModel : BetListExcelMetaModel, IRunningBetListMetaModel
    {
        public bool ExcludeFreeBet { get; set; }
    }
}