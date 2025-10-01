namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IExtraPayoutRepository
    {
        Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMainData(
            ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMemberData(
            ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayoutDetail>> GetExtraPayoutDetailData(
            ExtraPayoutDetailMetaModel metaModel);

        Task<SortableReportResultModel<OddsBoostBonusDetail>> GetOddsBoostBonusDetailData(
            ExtraPayoutMetaModel metaModel);
    }
}