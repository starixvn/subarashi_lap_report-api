namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface IExtraPayoutRepository
    {
        Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMainData(
            ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMemberData(
            ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayoutDetail>> GetExtraPayoutDetailData(
            ExtraPayoutDetailMetaModel metaModel);
    }
}